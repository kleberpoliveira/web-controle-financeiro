 
const Rotas = {
  home: () => '/home',
  homeComParametro: (i) => `/home/${i}`, // Nova rota dinâmica
  
  
  cadastros: {
    cartao: {
      listar: () => '/cadastros/cartao',
      salvar: () => '/cadastros/cartao',
      excluir: (id) => `/cadastros/cartao/${id}`
    },
    contaBancaria: {
      listar: () => '/cadastros/conta-bancaria',
      salvar: () => '/cadastros/conta-bancaria'
    },
    planoConta: {
      listar: () => '/cadastros/plano-conta'
    }
  },

  movimentacoes: {
    lancamentos: {
      listar: () => '/movimentacoes/lancamentos'
    },
    transferencias: {
      salvar: () => '/movimentacoes/transferencias'
    }
  }
};