# Release Notes - v0.2.0 (Finanças & Calendário) 📅⚽💰

Segunda versão oficial do **PES Master League Editor Ultra**, introduzindo o novo módulo de **Calendário da Master Liga**, permitindo manipular a data da carreira diretamente no save.

---

## 🌟 O Que Há de Novo na v0.2.0

### 📅 Novo Módulo de Calendário da Master Liga
- **Detecção Automática da Data**: Ao abrir qualquer save da Master Liga, o editor localiza automaticamente a data exata da carreira (dia, mês e ano).
- **Seletor de Data Visual (DateTimePicker)**: Escolha qualquer data do ano para o seu save.
- **Atalhos Rápidos de Simulação**:
  - `+ 1 Semana`: Avança o calendário em 7 dias com 1 clique.
  - `+ 1 Mês`: Avança o calendário em 30 dias.
  - `⏳ Fim Janela Verão (31/08)`: Salta imediatamente para o último dia da janela de transferências de verão.
- **Sincronização com os Metadados**: Atualiza simultaneamente o payload binário da Master Liga e o texto do slot exibido no menu do PES (`16/8/2018`).

### 💰 Finanças & Orçamentos (Consolidado)
- Ajuste direto do **Orçamento de Transferências** e **Caixa Total do Clube** (Orçamento de Salários).
- Atalhos rápidos para adicionar €50M, €100M ou maximizar valores com um clique.

### 🛡️ Proteção de Saves
- Geração automática de cópias de segurança `.bak` e `.bak_timestamp` antes de qualquer gravação.
- Botão dedicado de **Backup Manual** preventivo.

---

## 📦 Como Instalar e Usar

1. Baixe o pacote **`Pes2019MlEditor-v0.2.0-win-x64.zip`** anexado nesta release.
2. Descompacte o arquivo `.zip` em qualquer pasta.
3. Dê dois cliques em **`Pes2019MlEditor.UI.exe`** *(executável independente standalone, sem necessidade de instalar o .NET SDK)*.
4. Selecione seu save da Master Liga na lista de detecção automática.
5. Edite a **Data da Carreira** e os **Orçamentos**.
6. Clique em **Salvar Alterações** e carregue seu save no PES!
