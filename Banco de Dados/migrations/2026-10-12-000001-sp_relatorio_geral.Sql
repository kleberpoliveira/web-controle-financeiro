IF OBJECT_ID('dbo.sp_relatorio_geral', 'P') IS NOT NULL
BEGIN
    DROP PROCEDURE dbo.sp_relatorio_geral;
END
GO
CREATE PROCEDURE sp_relatorio_geral
@data date
AS
BEGIN

/*##########################################################################################################################################
													VARIAVEIS E TEMPORARIAS
##########################################################################################################################################*/

CREATE TABLE #GERAL ( 
	descricao VARCHAR(500),
	valor DECIMAL(12,2), 
	data DATE,
	forma_pagto VARCHAR(200),
	instituicao_pagamento VARCHAR(200),
	tipo_pagamento VARCHAR(200),
	tipo_compra VARCHAR(200)
)

CREATE TABLE #AGRUPADO (
	id BIGINT identity (1,1), 
	descricao VARCHAR(500),
	valor DECIMAL(12,2),
	periodo varchar(50),
	agrupador VARCHAR(200)
)

/*##########################################################################################################################################
													BUSCA OS DADOS CONFORME PERIODO
##########################################################################################################################################*/

INSERT INTO #GERAL
SELECT 
    cf_cab.descricao, 
    cf_parc.valor,  
	case when cbm.id_conta_bancaria_movto is not null  then cbm.data else cf_parc.vencimento end data,
    dbo.frg_forma_pagto(cbm.id_conta_bancaria_movto, cbm.tipo_opercao, fatura.id_fatura) AS frg_forma_pagto,
	dbo.frg_instituicao_pagamento(cbm.id_conta_bancaria_movto, cb.descricao, fatura.id_fatura, cartao.nome) AS instituicao_pagamento,
    dbo.frg_tipo_pagamento(cbm.id_conta_bancaria_movto, fatura.id_fatura) AS frg_tipo_pagamento,
    dbo.frg_tipo_compra(tipo_compra.qtd) AS frg_tipo_compra
FROM CF_CAB 
INNER JOIN cf_parc ON cf_parc.id_cf_cab = cf_cab.id_cf_cab
LEFT JOIN fatura ON fatura.id_fatura = cf_parc.id_fatura
LEFT JOIN cartao ON cartao.id_cartao = fatura.id_cartao
LEFT JOIN conta_bancaria_movto cbm ON cbm.id_conta_bancaria_movto = cf_parc.id_conta_bancaria_movto
LEFT JOIN conta_bancaria cb on cb.id_conta_bancaria = cbm.id_conta_bancaria
LEFT JOIN (select id_cf_cab, count(*) qtd from cf_parc group by id_cf_cab) AS tipo_compra ON tipo_compra.id_cf_cab = cf_parc.id_cf_cab
WHERE cf_cab.data >= @data;

--SELECT * FROM #GERAL

/*##########################################################################################################################################
											AGRUPA OS DADOS E COLOCA NA TEMP #AGRUPADO
##########################################################################################################################################*/


INSERT INTO #AGRUPADO
SELECT	
	instituicao_pagamento	AS DESCRICAO,
	SUM(valor)				AS VALOR,
	FORMAT(data, 'MM/yyyy') AS PERIODO,
	'CARTÃO'				AS AGRUPADOR
FROM #GERAL WHERE tipo_pagamento ='CARTAO'
GROUP BY FORMAT(data, 'MM/yyyy'), instituicao_pagamento

UNION

SELECT	
	'Outros saques e/ou pix'	AS DESCRICAO,
	SUM(valor)					AS VALOR,
	FORMAT(data, 'MM/yyyy')		AS PERIODO,
	'Outros saques e/ou pix'	AS AGRUPADOR
FROM #GERAL 
WHERE tipo_pagamento ='CONTA' 
AND tipo_compra = 'VISTA'
GROUP BY FORMAT(data, 'MM/yyyy') 

UNION

SELECT	
	descricao+'('+instituicao_pagamento+')'	AS DESCRICAO,
	SUM(valor)					AS VALOR,
	FORMAT(data, 'MM/yyyy')		AS PERIODO,
	forma_pagto					AS AGRUPADOR
FROM #GERAL 
WHERE tipo_pagamento ='CONTA' 
AND tipo_compra = 'PARCELADO' 
GROUP BY FORMAT(data, 'MM/yyyy'), forma_pagto, descricao ,instituicao_pagamento

UNION

SELECT 
	descricao					AS DESCRICAO,
	SUM(valor)					AS VALOR,
	FORMAT(data, 'MM/yyyy')		AS PERIODO,
	forma_pagto				AS AGRUPADOR
FROM #GERAL 
WHERE tipo_pagamento ='INDEFINIDO'
GROUP BY FORMAT(data, 'MM/yyyy'), forma_pagto , descricao

--SELECT * FROM #AGRUPADO  ORDER BY agrupador, descricao


/*##########################################################################################################################################
											MONT APIVOT COM RETORNO
##########################################################################################################################################*/


DECLARE @Colunas AS NVARCHAR(MAX) = '';
DECLARE @Query AS NVARCHAR(MAX) = '';

SELECT DISTINCT periodo INTO #PeriodosOrdenados FROM #AGRUPADO  WHERE periodo IS NOT NULL;

SELECT @Colunas = STUFF((
    SELECT ', [' + periodo + ']'
    FROM #PeriodosOrdenados
    ORDER BY SUBSTRING(periodo, 4, 4) + SUBSTRING(periodo, 1, 2)
    FOR XML PATH(''), TYPE
).value('.', 'NVARCHAR(MAX)'), 1, 2, '');

IF @Colunas IS NULL
    RETURN;

SET @Query = '
SELECT 
    agrupador,
    descricao, ' + @Colunas + '
FROM (
    SELECT agrupador, descricao, valor, periodo 
    FROM #AGRUPADO
) AS Dados
PIVOT (
    SUM(valor)
    FOR periodo IN (' + @Colunas + ')
) AS PivotTable
ORDER BY agrupador, descricao;';

EXEC sp_executesql @Query;

/*
========================================================
REGRAS DADAS FUNCOES 
========================================================

================================================================================================================
dbo.frg_forma_pagto(D.id_conta_bancaria_movto, D.tipo_opercao, C.id_fatura, E.nome) AS frg_forma_pagto,
================================================================================================================

CASE	WHEN D.id_conta_bancaria_movto IS NOT NULL THEN D.tipo_opercao 
		WHEN C.id_fatura IS NOT NULL THEN  E.nome 
		ELSE 'INDEFINIDO'  
		END AS forma_pagto, 

================================================================================================================
dbo.frg_tipo_pagamento(D.id_conta_bancaria_movto, C.id_fatura) AS frg_tipo_pagamento,
================================================================================================================

CASE	WHEN D.id_conta_bancaria_movto IS NOT NULL THEN 'CONTA'
		WHEN C.id_fatura IS NOT NULL THEN  'CARTAO' 
		ELSE 'INDEFINIDO' 
		END AS tipo_pagamento, 

================================================================================================================
dbo.frg_tipo_compra(tipo_compra.qtd) AS frg_tipo_compra
================================================================================================================

CASE	WHEN tipo_compra.qtd <=1 THEN 'VISTA' 
		ELSE 'PARCELADO' 
		END as tipo_compra 



================================================================================================================
dbo.frg_instituicao_pagamento(cbm.id_conta_bancaria_movto, cb.descricao, fatura.id_fatura, cartao.nome) AS instituicao_pagamento
================================================================================================================

CASE	WHEN cbm.id_conta_bancaria_movto IS NOT NULL THEN cb.descricao
		WHEN fatura.id_fatura IS NOT NULL THEN  cartao.nome
		ELSE 'INDEFINIDO' 
		END as instituicao_pagamento,


================================================================================================================

*/


END