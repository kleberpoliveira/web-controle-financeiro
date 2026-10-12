IF OBJECT_ID('dbo.sp_extrato_bancario', 'P') IS NOT NULL
BEGIN
    DROP PROCEDURE dbo.sp_extrato_bancario;
END
GO
CREATE PROCEDURE sp_extrato_bancario @id_conta_bancaria INT
AS
BEGIN
	SELECT 
		conta_bancaria_movto.data,
		conta_bancaria_movto.valor, 
		conta_bancaria_movto.tipo_movimento AS tipo_movimento,
		-- Regras condicionais usando os dados já trazidos pelo apply/joins
		CASE 
			-- 1. Se estiver vinculado à fatura
			WHEN fatura.id_fatura IS NOT NULL THEN 'Pagamento fatura cartão ' + cartao.nome
        
			-- 2. Se tiver exatamente 1 registro vinculado
			WHEN parcelas_info.total_parc = 1 THEN parcela_unica.descricao_cabecalho
        
			-- 3. Se tiver mais de 1 registro vinculado
			WHEN parcelas_info.total_parc > 1 THEN 'Pagamentos diversos'
		END AS descricao_movimento
	FROM conta_bancaria_movto
	-- Tenta encontrar se o movimento pagou uma fatura de cartão
	LEFT JOIN fatura ON fatura.id_conta_bancaria_movto = conta_bancaria_movto.id_conta_bancaria_movto
	LEFT JOIN cartao ON cartao.id_cartao = fatura.id_cartao
	-- Subconsulta rápida apenas para contar quantas parcelas estão ligadas
	LEFT JOIN (
		SELECT id_conta_bancaria_movto, COUNT(*) AS total_parc
		FROM cf_parc
		WHERE id_conta_bancaria_movto IS NOT NULL
		GROUP BY id_conta_bancaria_movto
	) AS parcelas_info ON parcelas_info.id_conta_bancaria_movto = conta_bancaria_movto.id_conta_bancaria_movto
	-- OUTER APPLY faz o papel de um JOIN inteligente que traz no máximo 1 linha por movimento, sem duplicar
	OUTER APPLY (
		SELECT TOP 1 cf_cab.descricao AS descricao_cabecalho
		FROM cf_parc
		INNER JOIN cf_cab ON cf_cab.id_cf_cab = cf_parc.id_cf_cab
		WHERE cf_parc.id_conta_bancaria_movto = conta_bancaria_movto.id_conta_bancaria_movto
	) AS parcela_unica
	WHERE id_conta_bancaria = @id_conta_bancaria
	ORDER BY conta_bancaria_movto.data ASC

END