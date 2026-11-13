< !DOCTYPE html >
< html lang = "pt-BR" >
< head >
  < meta charset = "UTF-8" >
  < meta name = "viewport" content = "width=device-width, initial-scale=1.0" >
  < link rel = "stylesheet" href = "/css/style.css" >

  < script src = "https://unpkg.com/vue@3/dist/vue.global.prod.js" ></ script >
  < script src = "/js/config.js" ></ script >
  < script src = "/js/utils.js" ></ script >
  < script src = "/js/rotas.js?v=6" ></ script >
  < script src = "/js/estrutura.js" ></ script >

  < script > document.write(`< title >${ TITULO_SITE}
Lançamentos </ title >`);</ script >
</ head >
< body >

< div id = "app" >
  < app - header ></ app - header >
  < main >
    < div >
      < div style = "margin-top: 20px;" >
        < label for= "select-plano" > Selecione o Plano de Conta:</ label >
        < br >
        < select id = "select-plano" form = "form-compra" required v - model = "planoSelecionado" style = "padding: 8px; width: 350px; margin-top: 5px;" >
          < !--Opções caso venham dados normais-- >
          < template v -if= "dados_plano_conta.length > 0" >
            < option disabled value = "" > Selecione uma opção</option>
            <option v-for="conta in dados_plano_conta" :key = "conta.id_plano_conta" :value = "conta.id_plano_conta" >
              { { conta.codigo } }
- { { conta.descricao } }
            </ option >
          </ template >

          < !--Opção exibida caso dê erro ou não encontre registros e já tenha terminado de carregar -->
          <template v-else-if="!carregando">
            <option disabled value="">Erro ao carregar o plano de contas</option>
          </template>

          <!-- Opção exibida enquanto estiver carregando -->
          <template v-else>
            <option disabled value="">Carregando planos de conta...</option>
          </template>
        </select>
      </div>
      <hr style="margin: 20px 0;">

      <form id="form-compra" @submit.prevent="salvarCompra" style="max-width: 400px;">
        <h3>Lançar Compra</h3>

        <div style="margin-bottom: 10px;">
          <label>Data:</ label >< br >
          < input type = "text" v - model = "form.Data" placeholder = "DD/MM/AAAA" required style = "width: 100%; padding: 6px;" >
        </ div >

        < div style="margin-bottom: 10px;">
          <label>Descrição:</ label >< br >
          < input type = "text" v - model = "form.Descricao" required style = "width: 100%; padding: 6px;" >
        </ div >

        < div style="margin-bottom: 10px;">
          <label>Total:</ label >< br >
          < input type = "number" step = "0.01" v - model.number = "form.Total" required style = "width: 100%; padding: 6px;" >
        </ div >

        < !--Tipo de compra (lista fixa) -->
        <div style="margin-bottom: 10px;">
          <label>Tipo de Compra:</ label >< br >
          < select v - model = "form.TipoDeCompra" required style = "width: 100%; padding: 6px;" >
            < option disabled value="">Selecione uma opção</option>
            <option value="CARTAO_CREDITO">Cartão de crédito</option>
            <option value="DEBITO_EM_CONTA">Débito em conta</option>
            <option value="PIX">PIX</option>
          </select>
        </div>

        <!-- Aparece só para cartão de crédito -->
        <div v-if="ehCartao" style="margin-bottom: 10px;">
          <label>Cartão:</ label >< br >
          < select v - model = "form.IdCartao" required style = "width: 100%; padding: 6px;" >
            < option disabled value="">Selecione uma opção</option>
            <option v-for="c in cartoes" :key = "idCartao(c)" :value = "idCartao(c)" >
              { { nomeCartao(c) } }
            </ option >
          </ select >
        </ div >

        < !--Aparece para débito em conta (obrigatório) e PIX (opcional) -->
        <div v-if="ehConta" style="margin-bottom: 10px;">
          <label>Conta Bancária<span v-if="!contaObrigatoria"> (opcional)</span>:</ label >< br >
          < select v - model = "form.IdContaBancaria" :required = "contaObrigatoria" style = "width: 100%; padding: 6px;" >
            < option v -if= "contaObrigatoria" disabled value = "" > Selecione uma opção</option>
            <option v-else value= "" > Nenhuma(definir depois) </ option >
            < option v-for="cb in contasBancarias" :key = "idConta(cb)" :value = "idConta(cb)" >
              { { nomeConta(cb) } }
            </ option >
          </ select >
        </ div >

        < !--Vale para qualquer tipo de compra -->
        <div style="margin-bottom: 15px;">
          <label>Quantidade de Parcelas:</ label >< br >
          < input type = "number" v - model.number = "form.QtdParcelas" min = "1" required style = "width: 100%; padding: 6px;" >
        </ div >

        < button type="submit" :disabled = "enviando" style = "padding: 10px 20px; background-color: #28a745; color: white; border: none; cursor: pointer;" >
          { { enviando ? 'Salvando...' : 'Salvar Compra' } }
        </ button >
      </ form >

    </ div >
  </ main >
  < app - footer ></ app - footer >

  < !--Modal global de erros (usa Utils.modalState) -->
  <app-modal
    :visivel = "Utils.modalState.visivel"
    :mensagem = "Utils.modalState.mensagem"
    @fechar = "Utils.fecharModal()" >
  </ app - modal >
</ div >

< script >
  const { createApp } = Vue;

const formVazio = () => ({
    Data: '',
    Descricao: '',
    Total: 0,
    TipoDeCompra: '',
    IdCartao: '',
    IdContaBancaria: '',
    QtdParcelas: 1
  });

const app = createApp({
    data() {
      return {

      dados_plano_conta: [],
      cartoes: [],
      contasBancarias: [],
      planoSelecionado: '',
      carregando: false,
      enviando: false,
      form: formVazio()
      };
    },
    computed:
{
    ehCartao() {
        return this.form.TipoDeCompra === 'CARTAO_CREDITO';
    },
      // Débito em conta e PIX mostram o select de conta bancária
      ehConta() {
        return this.form.TipoDeCompra === 'DEBITO_EM_CONTA' || this.form.TipoDeCompra === 'PIX';
    },
      // Só o débito em conta exige escolher a conta (no PIX é opcional)
      contaObrigatoria() {
        return this.form.TipoDeCompra === 'DEBITO_EM_CONTA';
    }
},
    watch:
{
    // Ao trocar o tipo de compra, limpa as seleções que dependem dele
    'form.TipoDeCompra'() {
        this.form.IdCartao = '';
        this.form.IdContaBancaria = '';
    }
},
    methods:
{
    // Aceitam o nome do campo em maiúsculas ou minúsculas, conforme vier do banco
    idCartao(c)   { return c.ID_CARTAO ?? c.id_cartao; },
      nomeCartao(c) { return c.NOME ?? c.nome; },
      idConta(cb)   { return cb.ID_CONTA_BANCARIA ?? cb.id_conta_bancaria; },
      nomeConta(cb) { return cb.DESCRICAO ?? cb.descricao; },

      async buscarDados()
    {
        this.carregando = true;
        try
        {
            const resposta = await Rotas.plano_conta.listar();
            if (Array.isArray(resposta))
            {
                this.dados_plano_conta = resposta;
            }
            else if (resposta && Array.isArray(resposta.dados))
            {
                this.dados_plano_conta = resposta.dados;
            }
            else if (resposta && Array.isArray(resposta.resultado))
            {
                this.dados_plano_conta = resposta.resultado;
            }
            else
            {
                this.dados_plano_conta = [];
            }
        }
        catch (error)
        {
            // O Utils.apiFetch já exibe o modal global com a mensagem do erro
            console.error("Erro ao buscar planos de contas:", error);
            this.dados_plano_conta = [];
        }
        finally
        {
            this.carregando = false;
        }
    },
      async buscarCartoes()
    {
        try
        {
            const resposta = await Rotas.cartao.listar();
            this.cartoes = Array.isArray(resposta) ? resposta : [];
        }
        catch (error)
        {
            console.error("Erro ao buscar cartões:", error);
            this.cartoes = [];
        }
    },
      async buscarContasBancarias()
    {
        try
        {
            const resposta = await Rotas.conta_bancaria.listar();
            this.contasBancarias = Array.isArray(resposta) ? resposta : [];
        }
        catch (error)
        {
            console.error("Erro ao buscar contas bancárias:", error);
            this.contasBancarias = [];
        }
    },
      async salvarCompra()
    {
        this.enviando = true;
        try
        {
            const payload = {
            Data: this.form.Data,
            Descricao: this.form.Descricao,
            Total: this.form.Total,
            TipoDeCompra: this.form.TipoDeCompra,
            IdCartao: this.ehCartao ? this.form.IdCartao : null,
            // No PIX, se nenhuma conta foi escolhida, vai null
            IdContaBancaria: (this.ehConta && this.form.IdContaBancaria !== '') ? this.form.IdContaBancaria : null,
            QtdParcelas: this.form.QtdParcelas,
            IdPlanoConta: this.planoSelecionado
          }
;
await Rotas.compra.lancarCompra(payload);
alert('Compra salva com sucesso!');
// Limpa o formulário e reseta o select
this.form = formVazio();
this.planoSelecionado = '';
        } catch (error) {
    // O Utils.apiFetch já exibe o modal global com a mensagem do erro
    console.error('Erro ao salvar compra:', error);
} finally {
    this.enviando = false;
}
      }
    },
    mounted() {
    this.buscarDados();
    this.buscarCartoes();
    this.buscarContasBancarias();
}
  });

app.config.globalProperties.Utils = Utils;

app.component('app-header', AppHeader);
app.component('app-footer', AppFooter);
app.component('app-modal', AppModal);
app.mount('#app');
</ script >
</ body >
</ html >