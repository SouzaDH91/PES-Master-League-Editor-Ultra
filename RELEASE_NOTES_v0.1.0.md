# Release Notes - v0.1.0 (Initial Public Release) ⚽💰

Primeira versão pública do **PES 2019 Master League Editor** para PC, trazendo o motor criptográfico nativo e o módulo completo de finanças e orçamentos da Master Liga.

---

## 🌟 Principais Recursos

### 🔓 Motor Criptográfico Nativo (C# / .NET)
- Implementação integral do algoritmo criptográfico da Konami baseado em **Mersenne Twister (MT19937)** com a Master Key do PES 2019.
- Compatibilidade total com arquivos de save da Master Liga (`ML00000000`, `ML00000001`, etc.).
- Descriptografia, validação de integridade e re-criptografia simétrica sem necessitar de ferramentas externas (Python, decrypters de terceiros, etc.).
- 100% de integridade confirmada in-game: o jogo carrega o save editado sem acusar "dados corrompidos".

### 💰 Gestão Completa de Finanças & Orçamentos
- **Orçamento de Transferências**: Permite definir exatamente a quantia desejada para contratações.
- **Caixa Total do Clube**: Modifique a conta corrente geral da equipe, expandindo a margem disponível para o **Orçamento de Salários**.
- **Botões de Ação Rápida**: Atalhos diretos para somar `+ €50M`, `+ €100M`, `+ €10M em Salários` ou maximizar o caixa com um clique.
- **Detecção Automática do Clube**: Localiza o registro financeiro exato do seu clube na Master Liga.

### 🛡️ Proteção & Backups Automáticos
- Toda vez que você clica em *Salvar Alterações*, o aplicativo cria:
  - Uma cópia de restauração imediata (`ML0000000X.bak`).
  - Um histórico versionado com carimbo de data e hora (`ML0000000X.bak_YYYYMMDD_HHMMSS`).
- Botão dedicado para criação de **Backup Manual** preventivo a qualquer momento.

### 🎨 Interface Gráfica Moderna (Dark Theme)
- Interface limpa e responsiva em Windows Forms com visual escuro.
- **Detecção Automática de Saves**: Localiza automaticamente os saves da Master Liga na pasta de documentos (Steam e patches comunitários como HANO4U).
- Exibição da miniatura (logo/thumbnail) gravada dentro do próprio save.
- Console de log em tempo real integrado no rodapé da janela.

---

## 📦 Como Instalar e Usar

1. Baixe o pacote **`Pes2019MlEditor-v0.1.0-win-x64.zip`** anexado nesta release.
2. Descompacte o arquivo `.zip` em qualquer pasta do seu computador.
3. Execute o aplicativo **`Pes2019MlEditor.UI.exe`** *(não é necessário instalar o .NET SDK runtime, o executável é standalone e autossuficiente)*.
4. Selecione o seu save da Master Liga na lista (ou use o botão **Procurar...**).
5. Ajuste os valores dos orçamentos como desejar.
6. Clique em **Salvar Alterações**.
7. Abra o **PES 2019** e divirta-se com seu novo orçamento!

---

## 📋 Arquitetura e Detalhes Técnicos
- Plataforma: **Windows x64**
- Framework: **.NET 10 (Windows Forms)**
- Arquivo Binário: `Pes2019MlEditor.UI.exe` (Single-File self-contained)

---

## 🗺️ Próximas Versões Planejadas
- **v0.2**: Edição de Calendário (data da temporada, avanço de semanas) e configurações da carreira.
- **v0.3**: Gestão do Elenco (contratos restantes, salários individuais de jogadores, moral e espírito de equipe).
- **v0.4**: Status e Atributos de Jogadores (overall, habilidades e fadiga).
- **v0.5**: Transferências Forçadas entre clubes diretamente no save.
- **v0.6**: Suporte Multi-Versões do PES (adaptação para PES 2016-2021 com chaves dedicadas e legado PES 2013-2015).
