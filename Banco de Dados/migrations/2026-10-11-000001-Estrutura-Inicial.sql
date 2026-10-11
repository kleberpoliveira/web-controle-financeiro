-- ==========================================
-- 1. CRIAÇÃO DAS TABELAS
-- ==========================================

-- Tabela: plano_conta
CREATE TABLE plano_conta (
    id_plano_conta INT IDENTITY(1,1) PRIMARY KEY,
    codigo VARCHAR(200) NOT NULL,
    descricao VARCHAR(100) NOT NULL
);

-- Tabela: conta_bancaria
CREATE TABLE conta_bancaria (
    id_conta_bancaria INT IDENTITY(1,1) PRIMARY KEY,
    descricao VARCHAR(50) NOT NULL
);

-- Tabela: cartao
CREATE TABLE cartao (
    id_cartao INT IDENTITY(1,1) PRIMARY KEY,
    nome VARCHAR(50) NOT NULL,
    dia_fechamento INT NOT NULL
);

-- Tabela: fatura
CREATE TABLE fatura (
    id_fatura INT IDENTITY(1,1) PRIMARY KEY,
    id_cartao INT NOT NULL,
    data_vencimento DATE NOT NULL,
    CONSTRAINT FK_fatura_cartao FOREIGN KEY (id_cartao) REFERENCES cartao(id_cartao)
);

-- Tabela: conta_bancaria_movto
CREATE TABLE conta_bancaria_movto (
    id_conta_bancaria_movto INT IDENTITY(1,1) PRIMARY KEY,
    id_conta_bancaria INT NOT NULL,
    data DATE NOT NULL,
    tipo_movimento VARCHAR(1) NOT NULL,
    tipo_opercao VARCHAR(100) NOT NULL,
    conciliado BIT NOT NULL DEFAULT 0,
    valor DECIMAL(12,2) NOT NULL,
    CONSTRAINT FK_conta_bancaria_movto_conta_bancaria FOREIGN KEY (id_conta_bancaria) REFERENCES conta_bancaria(id_conta_bancaria),
    -- Check Constraint para tipo_movimento (C ou D)
    CONSTRAINT CK_conta_bancaria_movto_tipo_movimento CHECK (tipo_movimento IN ('C', 'D')),
    -- Check Constraint para tipo_opercao
    CONSTRAINT CK_conta_bancaria_movto_tipo_operacao CHECK (tipo_opercao IN (
        'PIX_ENTRADA', 'PIX_SAIDA', 'DEPOSITO', 'SAQUE', 'TRANSFERENCIA', 'DEBITO_EM_CONTA', 'CARTAO_DEBITO'
    ))
);

-- Tabela: transferencia
CREATE TABLE transferencia (
    id_transferencia INT IDENTITY(1,1) PRIMARY KEY,
    id_conta_bancaria_movto_origem INT NOT NULL,
    id_conta_bancaria_movto_destino INT NOT NULL,
    valor DECIMAL(12,2) NOT NULL,
    CONSTRAINT FK_transferencia_movto_origem FOREIGN KEY (id_conta_bancaria_movto_origem) REFERENCES conta_bancaria_movto(id_conta_bancaria_movto),
    CONSTRAINT FK_transferencia_movto_destino FOREIGN KEY (id_conta_bancaria_movto_destino) REFERENCES conta_bancaria_movto(id_conta_bancaria_movto)
);

-- Tabela: crp (Contas a Receber e Pagar)
CREATE TABLE crp (
    id_crp INT IDENTITY(1,1) PRIMARY KEY,
    data DATE NOT NULL,
    descricao VARCHAR(200) NOT NULL,
    total DECIMAL(12,2) NOT NULL
);

-- Tabela: crp_parcela
CREATE TABLE crp_parcela (
    id_crp_parcela INT IDENTITY(1,1) PRIMARY KEY,
    id_crp INT NOT NULL,
    numero INT NOT NULL,
    id_conta_bancaria_movto INT NULL,
    id_fatura INT NULL,
    valor DECIMAL(12,2) NOT NULL,
    vencimento DATE NOT NULL,
    CONSTRAINT FK_crp_parcela_crp FOREIGN KEY (id_crp) REFERENCES crp(id_crp),
    CONSTRAINT FK_crp_parcela_conta_bancaria_movto FOREIGN KEY (id_conta_bancaria_movto) REFERENCES conta_bancaria_movto(id_conta_bancaria_movto),
    CONSTRAINT FK_crp_parcela_fatura FOREIGN KEY (id_fatura) REFERENCES fatura(id_fatura)
);

-- Tabela: crp_parcela_plano_conta
CREATE TABLE crp_parcela_plano_conta (
    id_crp_parcela_plano_conta INT IDENTITY(1,1) PRIMARY KEY,
    id_plano_conta INT NOT NULL,
    id_crp_parcela INT NOT NULL,
    valor DECIMAL(12,2) NOT NULL,
    CONSTRAINT FK_crp_parcela_plano_conta_plano_conta FOREIGN KEY (id_plano_conta) REFERENCES plano_conta(id_plano_conta),
    CONSTRAINT FK_crp_parcela_plano_conta_parcela FOREIGN KEY (id_crp_parcela) REFERENCES crp_parcela(id_crp_parcela)
);
GO

-- ==========================================
-- 2. INSERÇÃO DOS DADOS
-- ==========================================

-- Inserir contas bancárias
INSERT INTO conta_bancaria (descricao) VALUES 
('Santander'),
('Caixa Conta Corrente'),
('Caixa Poupança'),
('Nubank'),
('Neon');

-- Inserir cartões com dia de fechamento = 28
INSERT INTO cartao (nome, dia_fechamento) VALUES 
('Santander Visa', 28),
('Neon', 28),
('Cartão Caixa', 28),
('Magalu', 28);
GO