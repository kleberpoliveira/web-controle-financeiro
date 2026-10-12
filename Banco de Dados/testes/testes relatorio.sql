/*
declare @cf_cab int  = 4 

select * from cf_cab where id_cf_cab = @cf_cab

select * from cf_parc where id_cf_cab = @cf_cab

select * from fatura where id_fatura in (4,8)


 sp_relatorio_geral  '2026-01-01'
 
 sp_extrato_bancario 1

 */


DELETE FROM plano_conta
DELETE FROM conta_bancaria
DELETE FROM cartao
DELETE FROM fatura
DELETE FROM conta_bancaria_movto
DELETE FROM transferencia
DELETE FROM cf_cab
DELETE FROM cf_parc
DELETE FROM cf_parc_plano_conta
 
  
DBCC CHECKIDENT ('cf_parc_plano_conta', RESEED, 0);
DBCC CHECKIDENT ('cf_parc', RESEED, 0);
DBCC CHECKIDENT ('cf_cab', RESEED, 0);
DBCC CHECKIDENT ('transferencia', RESEED, 0);
DBCC CHECKIDENT ('conta_bancaria_movto', RESEED, 0);
DBCC CHECKIDENT ('fatura', RESEED, 0);
DBCC CHECKIDENT ('cartao', RESEED, 0);
DBCC CHECKIDENT ('conta_bancaria', RESEED, 0);
DBCC CHECKIDENT ('plano_conta', RESEED, 0);

 

-- ---------------------------------------------------------------------
-- 1. PLANOS DE CONTA (Garantindo planos diferentes para cada tipo)
-- ---------------------------------------------------------------------
INSERT INTO plano_conta (codigo, descricao) VALUES 
('1.1.01', 'Alimentação / Supermercado'),
('1.1.02', 'Eletrônicos e Utensílios'),
('2.1.01', 'PIX Realizado'),
('2.1.02', 'Saques Bancários'),
('2.1.03', 'Débitos Automáticos em Conta'),
('3.1.01', 'Despesas Diversas (Sem Vínculo)');

-- ---------------------------------------------------------------------
-- 2. CONTAS BANCÁRIAS E CARTÕES (Conforme cadastros anteriores)
-- ---------------------------------------------------------------------
INSERT INTO conta_bancaria (descricao) VALUES 
('Santander'),
('Caixa Conta Corrente'),
('Caixa Poupança'),
('Nubank'),
('Neon');

INSERT INTO cartao (nome, dia_fechamento) VALUES 
('Santander Visa', 28),
('Neon', 28),
('Cartão Caixa', 28),
('Magalu', 28);

-- Faturas vinculadas a cada um dos 4 cartões
INSERT INTO fatura (id_cartao, data_vencimento) VALUES 
((SELECT id_cartao FROM cartao WHERE nome = 'Santander Visa'), '2026-11-10'),
((SELECT id_cartao FROM cartao WHERE nome = 'Neon'), '2026-11-10'),
((SELECT id_cartao FROM cartao WHERE nome = 'Cartão Caixa'), '2026-11-10'),
((SELECT id_cartao FROM cartao WHERE nome = 'Magalu'), '2026-11-10'),

((SELECT id_cartao FROM cartao WHERE nome = 'Santander Visa'), '2026-12-10'),
((SELECT id_cartao FROM cartao WHERE nome = 'Neon'), '2026-12-10'),
((SELECT id_cartao FROM cartao WHERE nome = 'Neon'), '2027-01-10'),
((SELECT id_cartao FROM cartao WHERE nome = 'Magalu'), '2026-12-10');





-- ---------------------------------------------------------------------
-- 3. MOVIMENTAÇÕES BANCÁRIAS (Para PIX_SAIDA, SAQUE e DEBITO_EM_CONTA)
-- ---------------------------------------------------------------------
-- ID 1: PIX_SAIDA (Santander)
INSERT INTO conta_bancaria_movto (id_conta_bancaria, data, tipo_movimento, tipo_opercao, conciliado, valor) 
VALUES ((SELECT id_conta_bancaria FROM conta_bancaria WHERE descricao = 'Santander'), '2026-10-01', 'D', 'PIX_SAIDA', 1, 150.00);

-- ID 2: SAQUE (Caixa Conta Corrente)
INSERT INTO conta_bancaria_movto (id_conta_bancaria, data, tipo_movimento, tipo_opercao, conciliado, valor) 
VALUES ((SELECT id_conta_bancaria FROM conta_bancaria WHERE descricao = 'Santander'), '2026-10-02', 'D', 'SAQUE', 1, 200.00);

-- ID 3: DEBITO_EM_CONTA (Nubank)
INSERT INTO conta_bancaria_movto (id_conta_bancaria, data, tipo_movimento, tipo_opercao, conciliado, valor) 
VALUES ((SELECT id_conta_bancaria FROM conta_bancaria WHERE descricao = 'Santander'), '2026-10-03', 'D', 'DEBITO_EM_CONTA', 0, 89.90);

INSERT INTO conta_bancaria_movto (id_conta_bancaria, data, tipo_movimento, tipo_opercao, conciliado, valor) 
VALUES ((SELECT id_conta_bancaria FROM conta_bancaria WHERE descricao = 'Santander'), '2026-11-03', 'D', 'DEBITO_EM_CONTA', 0, 89.90);
 
INSERT INTO conta_bancaria_movto (id_conta_bancaria, data, tipo_movimento, tipo_opercao, conciliado, valor) 
VALUES ((SELECT id_conta_bancaria FROM conta_bancaria WHERE descricao = 'Santander'), '2026-11-25', 'D', 'SAQUE', 0, 200);

-- =====================================================================
-- 4. LANÇAMENTOS (cf_cab, cf_parc e cf_parc_plano_conta)
-- =====================================================================

-- ---------------------------------------------------------------------
-- CENÁRIO 1: Compras Parceladas no Cartão (uma em cada cartão: 2x, 3x, 4x)
-- ---------------------------------------------------------------------

-- A) Cartão 1: Santander Visa (2 parcelas)
INSERT INTO cf_cab (data, descricao, total) VALUES ('2026-10-05', 'Compra Parcelada - Santander Visa (2x)', 600.00);
DECLARE @id_cab_cartao1 INT = SCOPE_IDENTITY();

INSERT INTO cf_parc (id_cf_cab, numero, id_conta_bancaria_movto, id_fatura, valor, vencimento) VALUES 
(@id_cab_cartao1, 1, NULL, (SELECT TOP 1 id_fatura FROM fatura f INNER JOIN cartao c ON f.id_cartao = c.id_cartao WHERE c.nome = 'Santander Visa' and f.data_vencimento = '2026-11-10'), 300.00, '2026-11-10'),
(@id_cab_cartao1, 2, NULL, (SELECT TOP 1 id_fatura FROM fatura f INNER JOIN cartao c ON f.id_cartao = c.id_cartao WHERE c.nome = 'Santander Visa' and f.data_vencimento = '2026-12-10'), 300.00, '2026-12-10');

INSERT INTO cf_parc_plano_conta (id_plano_conta, id_cf_parc, valor) 
SELECT (SELECT id_plano_conta FROM plano_conta WHERE codigo = '1.1.02'), id_cf_parc, valor FROM cf_parc WHERE id_cf_cab = @id_cab_cartao1;


-- B) Cartão 2: Neon (3 parcelas)
INSERT INTO cf_cab (data, descricao, total) VALUES ('2026-10-05', 'Compra Parcelada - Neon (3x)', 900.00);
DECLARE @id_cab_cartao2 INT = SCOPE_IDENTITY();

INSERT INTO cf_parc (id_cf_cab, numero, id_conta_bancaria_movto, id_fatura, valor, vencimento) VALUES 
(@id_cab_cartao2, 1, NULL, (SELECT TOP 1 id_fatura FROM fatura f INNER JOIN cartao c ON f.id_cartao = c.id_cartao WHERE c.nome = 'Neon' and f.data_vencimento = '2026-11-10'), 300.00, '2026-11-10'),
(@id_cab_cartao2, 2, NULL, (SELECT TOP 1 id_fatura FROM fatura f INNER JOIN cartao c ON f.id_cartao = c.id_cartao WHERE c.nome = 'Neon' and f.data_vencimento = '2026-12-10'), 300.00, '2026-12-10'),
(@id_cab_cartao2, 3, NULL, (SELECT TOP 1 id_fatura FROM fatura f INNER JOIN cartao c ON f.id_cartao = c.id_cartao WHERE c.nome = 'Neon' and f.data_vencimento = '2027-01-10'), 300.00, '2027-01-10');

INSERT INTO cf_parc_plano_conta (id_plano_conta, id_cf_parc, valor) 
SELECT (SELECT id_plano_conta FROM plano_conta WHERE codigo = '1.1.02'), id_cf_parc, valor FROM cf_parc WHERE id_cf_cab = @id_cab_cartao2;
 


-- D) Cartão 4: Magalu (2 parcelas - como temos 4 cartões, ajustado para mais uma opção de teste)
INSERT INTO cf_cab (data, descricao, total) VALUES ('2026-10-05', 'Compra Parcelada - Magalu (2x)', 400.00);
DECLARE @id_cab_cartao4 INT = SCOPE_IDENTITY();

INSERT INTO cf_parc (id_cf_cab, numero, id_conta_bancaria_movto, id_fatura, valor, vencimento) VALUES 
(@id_cab_cartao4, 1, NULL, (SELECT TOP 1 id_fatura FROM fatura f INNER JOIN cartao c ON f.id_cartao = c.id_cartao WHERE c.nome = 'Magalu' and f.data_vencimento = '2026-11-10'), 200.00, '2026-11-10'),
(@id_cab_cartao4, 2, NULL, (SELECT TOP 1 id_fatura FROM fatura f INNER JOIN cartao c ON f.id_cartao = c.id_cartao WHERE c.nome = 'Magalu' and f.data_vencimento = '2026-12-10'), 200.00, '2026-12-10');

INSERT INTO cf_parc_plano_conta (id_plano_conta, id_cf_parc, valor) 
SELECT (SELECT id_plano_conta FROM plano_conta WHERE codigo = '1.1.02'), id_cf_parc, valor FROM cf_parc WHERE id_cf_cab = @id_cab_cartao4;


-- ---------------------------------------------------------------------
-- CENÁRIO 2: Vínculo com Conta Bancária (PIX_SAIDA, SAQUE, DEBITO_EM_CONTA)
-- ---------------------------------------------------------------------

-- A) Vinculado a PIX_SAIDA
INSERT INTO cf_cab (data, descricao, total) VALUES ('2026-10-01', 'Pagamento via PIX pai', 150.00);
DECLARE @id_cab_pix INT = SCOPE_IDENTITY();

INSERT INTO cf_parc (id_cf_cab, numero, id_conta_bancaria_movto, id_fatura, valor, vencimento) VALUES 
(@id_cab_pix, 1, (SELECT id_conta_bancaria_movto FROM conta_bancaria_movto WHERE tipo_opercao = 'PIX_SAIDA'), NULL, 150.00, '2026-10-01');

INSERT INTO cf_parc_plano_conta (id_plano_conta, id_cf_parc, valor) 
SELECT (SELECT id_plano_conta FROM plano_conta WHERE codigo = '2.1.01'), id_cf_parc, valor FROM cf_parc WHERE id_cf_cab = @id_cab_pix;


-- B) Vinculado a SAQUE
INSERT INTO cf_cab (data, descricao, total) VALUES ('2026-10-02', 'Retirada de Dinheiro / Saque', 200.00);
DECLARE @id_cab_saque INT = SCOPE_IDENTITY();

INSERT INTO cf_parc (id_cf_cab, numero, id_conta_bancaria_movto, id_fatura, valor, vencimento) VALUES 
(@id_cab_saque, 1, (SELECT id_conta_bancaria_movto FROM conta_bancaria_movto WHERE tipo_opercao = 'SAQUE' AND DATA <> '2026-11-25'), NULL, 200.00, '2026-10-02');

INSERT INTO cf_parc_plano_conta (id_plano_conta, id_cf_parc, valor) 
SELECT (SELECT id_plano_conta FROM plano_conta WHERE codigo = '2.1.02'), id_cf_parc, valor FROM cf_parc WHERE id_cf_cab = @id_cab_saque;


-- C) Vinculado a DEBITO_EM_CONTA
INSERT INTO cf_cab (data, descricao, total) VALUES ('2026-10-03', 'Conta de Luz ', 89.90);
DECLARE @id_cab_debito INT = SCOPE_IDENTITY();

INSERT INTO cf_parc (id_cf_cab, numero, id_conta_bancaria_movto, id_fatura, valor, vencimento) VALUES 
(@id_cab_debito, 1, (SELECT id_conta_bancaria_movto FROM conta_bancaria_movto WHERE tipo_opercao = 'DEBITO_EM_CONTA' and data = '2026-10-03'), NULL, 89.90, '2026-10-03');

INSERT INTO cf_parc_plano_conta (id_plano_conta, id_cf_parc, valor) 
SELECT (SELECT id_plano_conta FROM plano_conta WHERE codigo = '2.1.03'), id_cf_parc, valor FROM cf_parc WHERE id_cf_cab = @id_cab_debito and vencimento = '2026-10-03';



INSERT INTO cf_parc (id_cf_cab, numero, id_conta_bancaria_movto, id_fatura, valor, vencimento) VALUES 
(@id_cab_debito, 1, (SELECT id_conta_bancaria_movto FROM conta_bancaria_movto WHERE tipo_opercao = 'DEBITO_EM_CONTA'  and data = '2026-11-03'), NULL, 89.90, '2026-11-03');

INSERT INTO cf_parc_plano_conta (id_plano_conta, id_cf_parc, valor) 
SELECT (SELECT id_plano_conta FROM plano_conta WHERE codigo = '2.1.03'), id_cf_parc, valor FROM cf_parc WHERE id_cf_cab = @id_cab_debito and vencimento = '2026-11-03';


-- ---------------------------------------------------------------------
-- CENÁRIO 3: Lançamento em 5x sem vínculo com cartão ou conta bancária
-- ---------------------------------------------------------------------
INSERT INTO cf_cab (data, descricao, total) VALUES ('2026-10-10', 'Acordo / Prestador Externo (5x sem vínculo)', 500.00);
DECLARE @id_cab_livre INT = SCOPE_IDENTITY();

INSERT INTO cf_parc (id_cf_cab, numero, id_conta_bancaria_movto, id_fatura, valor, vencimento) VALUES 
(@id_cab_livre, 1, NULL, NULL, 100.00, '2026-11-10'),
(@id_cab_livre, 2, NULL, NULL, 100.00, '2026-12-10'),
(@id_cab_livre, 3, NULL, NULL, 100.00, '2027-01-10'),
(@id_cab_livre, 4, NULL, NULL, 100.00, '2027-02-10'),
(@id_cab_livre, 5, NULL, NULL, 100.00, '2027-03-10');

-- Associa cada parcela ao plano de conta de despesas diversas
INSERT INTO cf_parc_plano_conta (id_plano_conta, id_cf_parc, valor) 
SELECT (SELECT id_plano_conta FROM plano_conta WHERE codigo = '3.1.01'), id_cf_parc, valor FROM cf_parc WHERE id_cf_cab = @id_cab_livre;

GO



UPDATE CF_PARC SET id_conta_bancaria_movto = (SELECT id_conta_bancaria_movto FROM conta_bancaria_movto WHERE tipo_opercao = 'SAQUE' AND data = '2026-11-25' )
WHERE id_cf_parc in (
	select id_cf_parc from cf_parc where id_cf_cab in (select id_cf_cab from cf_cab where descricao = 'Acordo / Prestador Externo (5x sem vínculo)')
)and numero in (1,2)
