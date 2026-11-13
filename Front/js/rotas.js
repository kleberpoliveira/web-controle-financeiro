const Rotas = {

    home: {
        get: () => '/home',
        async post(payload) {
            return await Utils.postJson('/home', payload);
        }
    },

    plano_conta: {
        async listar() {
            return await Utils.apiFetch('/plano_conta/listar');
        }
    },

    cartao: {
        async listar() {
            return await Utils.apiFetch('/cartao/listar');
        }
    },

    conta_bancaria: {
        async listar() {
            return await Utils.apiFetch('/conta_bancaria/listar');
        }
    },

    compra: {
        async lancarCompra(payload) {
            return await Utils.postJson('/compra/lancarcompra', payload);
        }
    }

};