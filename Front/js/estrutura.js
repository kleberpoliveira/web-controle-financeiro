// Componente Header Global com Menu Responsivo e Submenus
const AppHeader = {
  template: `
    <header class="app-header">
      <!-- Botão do Menu Hambúrguer (Celular) -->
      <button class="hamburger-btn" @click="toggleMobileMenu" aria-label="Menu">
        <span></span>
        <span></span>
        <span></span>
      </button>

      <!-- Navegação principal alinhada à esquerda -->
      <nav :class="['nav-menu', { 'active': mobileMenuOpen }]">
        <span class="home-link"><a href="/index.html" @click="closeMenus">Home</a></span>

        <!-- Dropdown: Cadastros -->
        <div class="dropdown" :class="{ 'open': dropdownCadastros }">
          <button class="dropdown-toggle" @click="toggleDropdown('cadastros')">
            Cadastros ▼
          </button>
          <div class="dropdown-menu">
            <a href="/cadastros/cartao.html" @click="closeMenus">Cartão de Crédito</a>
            <a href="/cadastros/conta_bancaria.html" @click="closeMenus">Conta bancária</a>
            <a href="/cadastros/plano_conta.html" @click="closeMenus">Plano de Contas</a>
          </div>
        </div>

        <!-- Dropdown: Movimentações -->
        <div class="dropdown" :class="{ 'open': dropdownMovimentacoes }">
          <button class="dropdown-toggle" @click="toggleDropdown('movimentacoes')">
            Movimentações ▼
          </button>
          <div class="dropdown-menu">
            <a href="/movimentacoes/lancamentos.html" @click="closeMenus">Lançamentos Diversos</a>
            <a href="/movimentacoes/transferencias.html" @click="closeMenus">Transferências entre contas</a>
          </div>
        </div>

        <!-- Dropdown: Relatórios -->
        <div class="dropdown" :class="{ 'open': dropdownRelatorios }">
          <button class="dropdown-toggle" @click="toggleDropdown('relatorios')">
            Relatórios ▼
          </button>
          <div class="dropdown-menu">
            <a href="/relatorios/movimentos_do_cartao.html" @click="closeMenus">Movimentos do Cartão</a>
            <a href="/relatorios/movimentos_da_conta.html" @click="closeMenus">Movimentos da conta corrente</a>
          </div>
        </div>
      </nav>
    </header>
  `,
  data() {
    return {
      mobileMenuOpen: false,
      dropdownCadastros: false,
      dropdownMovimentacoes: false,
      dropdownRelatorios: false
    };
  },
  methods: {
    toggleMobileMenu() {
      this.mobileMenuOpen = !this.mobileMenuOpen;
    },
    toggleDropdown(menu) {
      if (menu === 'cadastros') {
        this.dropdownCadastros = !this.dropdownCadastros;
        this.dropdownMovimentacoes = false;
        this.dropdownRelatorios = false;
      } else if (menu === 'movimentacoes') {
        this.dropdownMovimentacoes = !this.dropdownMovimentacoes;
        this.dropdownCadastros = false;
        this.dropdownRelatorios = false;
      } else if (menu === 'relatorios') {
        this.dropdownRelatorios = !this.dropdownRelatorios;
        this.dropdownCadastros = false;
        this.dropdownMovimentacoes = false;
      }
    },
    closeMenus() {
      this.mobileMenuOpen = false;
      this.dropdownCadastros = false;
      this.dropdownMovimentacoes = false;
      this.dropdownRelatorios = false;
    }
  }
};

const AppFooter = {
  template: `
    <footer>
      <p>&copy; 2026 {{ nomeSite }} - Todos os direitos reservados</p>
    </footer>
  `,
  data() {
    return {
      nomeSite: "Controle Financeiro"
    }
  }
};

const AppModal = {
  props: ['mensagem', 'visivel'],
  template: `
    <div v-if="visivel" style="position: fixed; top: 0; left: 0; width: 100%; height: 100%; background: rgba(0,0,0,0.5); display: flex; align-items: center; justify-content: center; z-index: 9999;">
      <div style="background: white; padding: 25px; border-radius: 8px; width: 400px; box-shadow: 0 4px 10px rgba(0,0,0,0.3); text-align: center;">
        <h3 style="color: #d9534f; margin-top: 0;">Atenção</h3>
        <p style="margin: 20px 0; color: #333; font-size: 16px;">{{ mensagem }}</p>
        <button @click="$emit('fechar')" style="background: #0275d8; color: white; border: none; padding: 10px 20px; border-radius: 4px; cursor: pointer; font-size: 14px;">
          Fechar
        </button>
      </div>
    </div>
  `
};