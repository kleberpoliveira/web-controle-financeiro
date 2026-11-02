// js/utils.js
// Exemplo de como estruturar no seu utils.js existente:
const Utils = {

  
  // Tornamos o estado reativo para o Vue detectar a mudança instantaneamente
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

      if (!response.ok) {
        throw new Error(`Erro na API: ${response.status}`);
      }

      return await response.json();
    } catch (error) { 
      // Agora a modal vai pipocar na tela bonitinha!
      this.exibirModal("Não foi possível comunicar com o servidor.");
      throw error;
    }
  },
  
    // Método padronizado para requisições POST com JSON
    async postJson(url, dados) {
        return await Utils.apiFetch(url, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(dados)
        });
    },

  
};