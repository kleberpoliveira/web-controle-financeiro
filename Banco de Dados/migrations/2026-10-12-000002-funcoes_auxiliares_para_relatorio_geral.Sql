CREATE OR ALTER FUNCTION dbo.frg_forma_pagto
(
    @id_conta_bancaria_movto INT, 
    @tipo_opercao VARCHAR(50),
    @id_fatura INT 
)
RETURNS VARCHAR(100)
AS 
BEGIN
    DECLARE @resultado VARCHAR(100);

    SET @resultado = CASE 
        WHEN @id_conta_bancaria_movto IS NOT NULL THEN @tipo_opercao 
        WHEN @id_fatura IS NOT NULL THEN 'CARTAO'
        ELSE 'INDEFINIDO' 
    END;

    RETURN @resultado;
END;
GO


CREATE OR ALTER FUNCTION dbo.frg_instituicao_pagamento(@id_conta_bancaria_movto INT, @descricao_conta VARCHAR(200), @id_fatura INT, @nome_cartao varchar(200))
RETURNS VARCHAR(50)
AS
BEGIN
    DECLARE @resultado VARCHAR(50);

    SET @resultado = CASE 
        WHEN @id_conta_bancaria_movto IS NOT NULL THEN @descricao_conta
        WHEN @id_fatura IS NOT NULL THEN @nome_cartao
        ELSE 'INDEFINIDO' 
    END;

    RETURN @resultado;
END;
GO


CREATE OR ALTER FUNCTION dbo.frg_tipo_pagamento(@id_conta_bancaria_movto INT, @id_fatura INT)
RETURNS VARCHAR(50)
AS
BEGIN
    DECLARE @resultado VARCHAR(50);

    SET @resultado = CASE 
        WHEN @id_conta_bancaria_movto IS NOT NULL THEN 'CONTA'
        WHEN @id_fatura IS NOT NULL THEN 'CARTAO' 
        ELSE 'INDEFINIDO' 
    END;

    RETURN @resultado;
END;
GO

CREATE OR ALTER FUNCTION dbo.frg_tipo_compra(@qtd_parcelas INT)
RETURNS VARCHAR(50)
AS
BEGIN
    DECLARE @resultado VARCHAR(50);

    SET @resultado = CASE 
        WHEN @qtd_parcelas <= 1 THEN 'VISTA' 
        ELSE 'PARCELADO' 
    END;

    RETURN @resultado;
END;
GO