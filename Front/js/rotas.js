class HomeParameters {
    constructor(data) {
        this.Data = data;
    }
}

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

    
    compra: {
        async lancarCompra(payload) {
        return await Utils.postJson('/compra/lancarcompra', payload);
    }
    },

    

};