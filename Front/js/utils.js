// /js/utils.js
const Utils = {

  modalState: Vue.reactive({
    visivel: false,
    mensagem: ''
  }),

  exibirModal(mensagem) {
    this.modalState.mensagem = mensagem;
    this.modalState.visivel = true;
  },

  fecharModal() {
    this.modalState.visivel = false;
    this.modalState.mensagem = '';
  },

  formatarMoeda(valor) {
    if (valor == null) return 'R$ 0,00';
    return valor.toLocaleString('pt-BR', {
      style: 'currency',
      currency: 'BRL',
      minimumFractionDigits: 2
    });
  },
  
  formatarValor(valor) {
      if (valor == null || valor === '') return '0,00';
      const numero = Number(valor);
      if (isNaN(numero)) return '0,00';
      return numero.toLocaleString('pt-BR', {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
      });
    },

  formatarData(data) {
    if (!data) return '';
    return new Date(data).toLocaleDateString('pt-BR');
  },

  extrairMensagemErro(respostaTexto, status) {
    // Novo padrão da API: JSON { "mensagem": "..." }
    try {
      const json = JSON.parse(respostaTexto);
      if (json && json.mensagem) return json.mensagem;
    } catch { /* não era JSON, segue para o padrão antigo */ }

    // Padrão antigo: texto da exceção com stack trace
    const mensagemLimpa = respostaTexto.split(' at ')[0].trim();
    return mensagemLimpa || `Erro na API: ${status}`;
  },

  async apiFetch(endpoint, options = {}) {
    console.log(endpoint);
    try {
      const response = await fetch(`${API_URL}${endpoint}`, {
        headers: {
          "Content-Type": "application/json",
          ...options.headers
        },
        ...options
      });

      const respostaTexto = await response.text();

      if (!response.ok) {
        throw new Error(this.extrairMensagemErro(respostaTexto, response.status));
      }

      // Se a resposta estiver vazia, retorna null/sucesso sem quebrar no JSON
      if (!respostaTexto) {
        return null;
      }

      return JSON.parse(respostaTexto);
    } catch (error) {
      // Falha de rede (API desligada, CORS, certificado): o fetch lança TypeError
      const erroFinal = (error instanceof TypeError)
        ? new Error("Não foi possível conectar ao servidor.")
        : error;

      this.exibirModal(erroFinal.message);
      throw erroFinal;
    }
  },

  async postJson(url, dados) {
      return await Utils.apiFetch(url, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(dados)
      });
  }
};