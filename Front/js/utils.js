const Utils = {
  formatarMoeda(valor) {
    if (valor == null) return 'R$ 0,00';
    return valor.toLocaleString('pt-BR', {
      style: 'currency',
      currency: 'BRL',
      minimumFractionDigits: 2
    });
  },
  
  formatarData(data) {
    if (!data) return '';
    return new Date(data).toLocaleDateString('pt-BR');
  },

  async apiFetch(endpoint, options = {}) {
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
      console.error("Falha na requisição:", error);
      alert("Não foi possível comunicar com o servidor.");
      throw error;
    }
  }
};