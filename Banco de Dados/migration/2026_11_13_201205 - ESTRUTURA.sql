 
CREATE TABLE [dbo].[cartao](
	[id_cartao] [int] IDENTITY(1,1) NOT NULL,
	[nome] [varchar](50) NOT NULL,
	[dia_fechamento] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id_cartao] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[cf_cab]    Script Date: 13/11/2026 19:58:51 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[cf_cab](
	[id_cf_cab] [int] IDENTITY(1,1) NOT NULL,
	[data] [date] NOT NULL,
	[descricao] [varchar](200) NOT NULL,
	[total] [decimal](12, 2) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id_cf_cab] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[cf_parc]    Script Date: 13/11/2026 19:58:51 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[cf_parc](
	[id_cf_parc] [int] IDENTITY(1,1) NOT NULL,
	[id_cf_cab] [int] NOT NULL,
	[numero] [int] NOT NULL,
	[id_conta_bancaria_movto] [int] NULL,
	[id_fatura] [int] NULL,
	[valor] [decimal](12, 2) NOT NULL,
	[vencimento] [date] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id_cf_parc] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[cf_parc_plano_conta]    Script Date: 13/11/2026 19:58:51 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[cf_parc_plano_conta](
	[id_cf_parc_plano_conta] [int] IDENTITY(1,1) NOT NULL,
	[id_plano_conta] [int] NOT NULL,
	[id_cf_parc] [int] NOT NULL,
	[valor] [decimal](12, 2) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id_cf_parc_plano_conta] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[conta_bancaria]    Script Date: 13/11/2026 19:58:51 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[conta_bancaria](
	[id_conta_bancaria] [int] IDENTITY(1,1) NOT NULL,
	[descricao] [varchar](50) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id_conta_bancaria] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[conta_bancaria_movto]    Script Date: 13/11/2026 19:58:51 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[conta_bancaria_movto](
	[id_conta_bancaria_movto] [int] IDENTITY(1,1) NOT NULL,
	[id_conta_bancaria] [int] NOT NULL,
	[data] [date] NOT NULL,
	[tipo_movimento] [varchar](1) NOT NULL,
	[tipo_opercao] [varchar](100) NOT NULL,
	[conciliado] [bit] NOT NULL,
	[valor] [decimal](12, 2) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id_conta_bancaria_movto] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[fatura]    Script Date: 13/11/2026 19:58:51 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[fatura](
	[id_fatura] [int] IDENTITY(1,1) NOT NULL,
	[id_cartao] [int] NOT NULL,
	[data_vencimento] [date] NOT NULL,
	[id_conta_bancaria_movto] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[id_fatura] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[plano_conta]    Script Date: 13/11/2026 19:58:51 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[plano_conta](
	[id_plano_conta] [int] IDENTITY(1,1) NOT NULL,
	[codigo] [varchar](200) NOT NULL,
	[descricao] [varchar](100) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id_plano_conta] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[transferencia]    Script Date: 13/11/2026 19:58:51 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[transferencia](
	[id_transferencia] [int] IDENTITY(1,1) NOT NULL,
	[id_conta_bancaria_movto_origem] [int] NOT NULL,
	[id_conta_bancaria_movto_destino] [int] NOT NULL,
	[valor] [decimal](12, 2) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id_transferencia] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[conta_bancaria_movto] ADD  DEFAULT ((0)) FOR [conciliado]
GO
ALTER TABLE [dbo].[cf_parc]  WITH CHECK ADD  CONSTRAINT [FK_cf_parc_cf_cab] FOREIGN KEY([id_cf_cab])
REFERENCES [dbo].[cf_cab] ([id_cf_cab])
GO
ALTER TABLE [dbo].[cf_parc] CHECK CONSTRAINT [FK_cf_parc_cf_cab]
GO
ALTER TABLE [dbo].[cf_parc]  WITH CHECK ADD  CONSTRAINT [FK_cf_parc_conta_bancaria_movto] FOREIGN KEY([id_conta_bancaria_movto])
REFERENCES [dbo].[conta_bancaria_movto] ([id_conta_bancaria_movto])
GO
ALTER TABLE [dbo].[cf_parc] CHECK CONSTRAINT [FK_cf_parc_conta_bancaria_movto]
GO
ALTER TABLE [dbo].[cf_parc]  WITH CHECK ADD  CONSTRAINT [FK_cf_parc_fatura] FOREIGN KEY([id_fatura])
REFERENCES [dbo].[fatura] ([id_fatura])
GO
ALTER TABLE [dbo].[cf_parc] CHECK CONSTRAINT [FK_cf_parc_fatura]
GO
ALTER TABLE [dbo].[cf_parc_plano_conta]  WITH CHECK ADD  CONSTRAINT [FK_cf_parc_plano_conta_cf_parc] FOREIGN KEY([id_cf_parc])
REFERENCES [dbo].[cf_parc] ([id_cf_parc])
GO
ALTER TABLE [dbo].[cf_parc_plano_conta] CHECK CONSTRAINT [FK_cf_parc_plano_conta_cf_parc]
GO
ALTER TABLE [dbo].[cf_parc_plano_conta]  WITH CHECK ADD  CONSTRAINT [FK_cf_parc_plano_conta_plano_conta] FOREIGN KEY([id_plano_conta])
REFERENCES [dbo].[plano_conta] ([id_plano_conta])
GO
ALTER TABLE [dbo].[cf_parc_plano_conta] CHECK CONSTRAINT [FK_cf_parc_plano_conta_plano_conta]
GO
ALTER TABLE [dbo].[conta_bancaria_movto]  WITH CHECK ADD  CONSTRAINT [FK_conta_bancaria_movto_conta_bancaria] FOREIGN KEY([id_conta_bancaria])
REFERENCES [dbo].[conta_bancaria] ([id_conta_bancaria])
GO
ALTER TABLE [dbo].[conta_bancaria_movto] CHECK CONSTRAINT [FK_conta_bancaria_movto_conta_bancaria]
GO
ALTER TABLE [dbo].[fatura]  WITH CHECK ADD  CONSTRAINT [FK_fatura_cartao] FOREIGN KEY([id_cartao])
REFERENCES [dbo].[cartao] ([id_cartao])
GO
ALTER TABLE [dbo].[fatura] CHECK CONSTRAINT [FK_fatura_cartao]
GO
ALTER TABLE [dbo].[fatura]  WITH CHECK ADD  CONSTRAINT [fk_fatura_conta_bancaria_movto] FOREIGN KEY([id_conta_bancaria_movto])
REFERENCES [dbo].[conta_bancaria_movto] ([id_conta_bancaria_movto])
GO
ALTER TABLE [dbo].[fatura] CHECK CONSTRAINT [fk_fatura_conta_bancaria_movto]
GO
ALTER TABLE [dbo].[transferencia]  WITH CHECK ADD  CONSTRAINT [FK_transferencia_movto_destino] FOREIGN KEY([id_conta_bancaria_movto_destino])
REFERENCES [dbo].[conta_bancaria_movto] ([id_conta_bancaria_movto])
GO
ALTER TABLE [dbo].[transferencia] CHECK CONSTRAINT [FK_transferencia_movto_destino]
GO
ALTER TABLE [dbo].[transferencia]  WITH CHECK ADD  CONSTRAINT [FK_transferencia_movto_origem] FOREIGN KEY([id_conta_bancaria_movto_origem])
REFERENCES [dbo].[conta_bancaria_movto] ([id_conta_bancaria_movto])
GO
ALTER TABLE [dbo].[transferencia] CHECK CONSTRAINT [FK_transferencia_movto_origem]
GO
ALTER TABLE [dbo].[conta_bancaria_movto]  WITH CHECK ADD  CONSTRAINT [CK_conta_bancaria_movto_tipo_movimento] CHECK  (([tipo_movimento]='D' OR [tipo_movimento]='C'))
GO
ALTER TABLE [dbo].[conta_bancaria_movto] CHECK CONSTRAINT [CK_conta_bancaria_movto_tipo_movimento]
GO
ALTER TABLE [dbo].[conta_bancaria_movto]  WITH CHECK ADD  CONSTRAINT [CK_conta_bancaria_movto_tipo_operacao] CHECK  (([tipo_opercao]='CARTAO_DEBITO' OR [tipo_opercao]='DEBITO_EM_CONTA' OR [tipo_opercao]='TRANSFERENCIA' OR [tipo_opercao]='SAQUE' OR [tipo_opercao]='DEPOSITO' OR [tipo_opercao]='PIX_SAIDA' OR [tipo_opercao]='PIX_ENTRADA'))
GO
ALTER TABLE [dbo].[conta_bancaria_movto] CHECK CONSTRAINT [CK_conta_bancaria_movto_tipo_operacao]
GO
