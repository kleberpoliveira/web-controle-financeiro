class HomeParameters {
    constructor(data) {
        this.Data = data;
    }
}

const Rotas = {

    // HOME
    home: () => '/home',

    async homePost(payload) {
        return await Utils.postJson('/home', payload);
    }
};