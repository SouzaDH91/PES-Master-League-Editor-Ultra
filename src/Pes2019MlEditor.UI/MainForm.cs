using System.Buffers.Binary;
using Pes2019MlEditor.Core.Calendar;
using Pes2019MlEditor.Core.Crypto;
using Pes2019MlEditor.Core.Finance;
using Pes2019MlEditor.Core.IO;
using Pes2019MlEditor.Core.Models;
using Pes2019MlEditor.Core.Squad;

namespace Pes2019MlEditor.UI;

public partial class MainForm : Form
{
    // State
    private PesSaveDescriptor? _currentSave;
    private string? _currentFilePath;
    private FinanceCandidate? _selectedCandidate;
    private string? _detectedSaveDirectory;
    private Dictionary<int, string> _editPlayerNames = [];
    private List<MlPlayerEntry> _currentSquad = [];

    // Colors - Modern Dark Theme
    private static readonly Color ColorBgDark = Color.FromArgb(18, 18, 26);
    private static readonly Color ColorSidebar = Color.FromArgb(24, 25, 38);
    private static readonly Color ColorCard = Color.FromArgb(32, 34, 50);
    private static readonly Color ColorInputBg = Color.FromArgb(40, 43, 62);
    private static readonly Color ColorAccent = Color.FromArgb(0, 180, 255);
    private static readonly Color ColorSuccess = Color.FromArgb(46, 204, 113);
    private static readonly Color ColorWarning = Color.FromArgb(241, 196, 15);
    private static readonly Color ColorText = Color.FromArgb(235, 238, 245);
    private static readonly Color ColorTextMuted = Color.FromArgb(145, 155, 175);
    private static readonly Color ColorBorder = Color.FromArgb(55, 60, 85);

    // Sidebar & Navigation
    private Panel _sidebarPanel = null!;
    private Panel _contentPanel = null!;
    private Button _btnNavHome = null!;
    private Button _btnNavFinance = null!;
    private Button _btnNavCalendar = null!;
    private Button _btnNavSquad = null!;
    private Button _btnNavLogs = null!;
    private Label _lblActiveSaveIndicator = null!;

    // Views (Panels)
    private Panel _viewHome = null!;
    private Panel _viewFinance = null!;
    private Panel _viewCalendar = null!;
    private Panel _viewSquad = null!;
    private Panel _viewLogs = null!;

    // Header / Save Controls
    private ComboBox _cboAutoSaves = null!;
    private Button _btnLoadAutoSave = null!;
    private Button _btnRefreshAutoSaves = null!;
    private TextBox _txtManualFilePath = null!;
    private Button _btnBrowseManual = null!;
    private Button _btnLoadManual = null!;
    private Label _lblStatus = null!;
    private Label _lblFilePath = null!;
    private PictureBox _picSaveLogo = null!;
    private Button _btnSaveFile = null!;
    private Button _btnBackup = null!;

    // Calendar Controls (v0.2)
    private DateTimePicker _dtpCareerDate = null!;
    private Button _btnApplyDate = null!;
    private Button _btnAdvanceWeek = null!;
    private Button _btnAdvanceMonth = null!;
    private Button _btnJumpTransferEnd = null!;
    private Label _lblCurrentCareerDate = null!;

    // Finance Controls
    private TextBox _txtTransferCurrent = null!;
    private TextBox _txtSalaryCurrent = null!;
    private Button _btnSearchBudget = null!;
    private ComboBox _cboCandidates = null!;
    private NumericUpDown _numNewTransfer = null!;
    private NumericUpDown _numNewSalary = null!;
    private Button _btnApplyBudget = null!;

    // Squad Controls (v0.3)
    private DataGridView _gridSquad = null!;
    private NumericUpDown _numTeamSpirit = null!;
    private Button _btnApplyTeamSpirit = null!;
    private Label _lblSquadCount = null!;
    private Label _lblSelectedPlayerInfo = null!;
    private NumericUpDown _numPlayerSalaryScaled = null!;
    private Label _lblPlayerSalaryEurEst = null!;
    private Button _btnApplyPlayerSalary = null!;

    // Logs Control
    private TextBox _txtLog = null!;

    public MainForm()
    {
        InitializeModernLayout();
        AutoDetectFolderAndPopulateSaves();
        SwitchView(_viewHome, _btnNavHome);
    }

    private void InitializeModernLayout()
    {
        Text = "PES Master League Editor Ultra v0.3.0 - Professional Suite";
        Size = new Size(1060, 740);
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1000, 680);
        BackColor = ColorBgDark;
        ForeColor = ColorText;
        Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);

        // Main Container: Sidebar (Left) + Content (Right)
        _sidebarPanel = new Panel
        {
            Dock = DockStyle.Left,
            Width = 220,
            BackColor = ColorSidebar,
            Padding = new Padding(0)
        };

        _contentPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = ColorBgDark,
            Padding = new Padding(15)
        };

        BuildSidebar();
        BuildViews();

        Controls.Add(_contentPanel);
        Controls.Add(_sidebarPanel);
    }

    private void BuildSidebar()
    {
        // App Title / Branding Header
        var pnlBrand = new Panel
        {
            Dock = DockStyle.Top,
            Height = 100,
            Padding = new Padding(15, 18, 15, 10)
        };

        var lblBrandTitle = new Label
        {
            Text = "⚽ PES ML ULTRA",
            Font = new Font("Segoe UI", 12.5f, FontStyle.Bold),
            ForeColor = ColorAccent,
            Dock = DockStyle.Top,
            Height = 32
        };

        var lblBrandSub = new Label
        {
            Text = "Master League Suite v0.3.0",
            Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
            ForeColor = ColorTextMuted,
            Dock = DockStyle.Top,
            Height = 22
        };

        _lblActiveSaveIndicator = new Label
        {
            Text = "Nenhum save aberto",
            Font = new Font("Segoe UI", 8f, FontStyle.Italic),
            ForeColor = ColorWarning,
            Dock = DockStyle.Bottom,
            Height = 20
        };

        pnlBrand.Controls.AddRange([_lblActiveSaveIndicator, lblBrandSub, lblBrandTitle]);

        // Navigation Buttons
        var pnlNav = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(8, 10, 8, 10)
        };

        _btnNavHome = CreateNavButton("🏠  Início / Save", 0);
        _btnNavFinance = CreateNavButton("💰  Finanças & Orçamento", 46);
        _btnNavCalendar = CreateNavButton("📅  Calendário da Carreira", 92);
        _btnNavSquad = CreateNavButton("👥  Gestão do Elenco", 138);
        _btnNavLogs = CreateNavButton("📜  Terminal & Logs", 184);

        _btnNavHome.Click += (s, e) => SwitchView(_viewHome, _btnNavHome);
        _btnNavFinance.Click += (s, e) => SwitchView(_viewFinance, _btnNavFinance);
        _btnNavCalendar.Click += (s, e) => SwitchView(_viewCalendar, _btnNavCalendar);
        _btnNavSquad.Click += (s, e) => SwitchView(_viewSquad, _btnNavSquad);
        _btnNavLogs.Click += (s, e) => SwitchView(_viewLogs, _btnNavLogs);

        pnlNav.Controls.AddRange([_btnNavHome, _btnNavFinance, _btnNavCalendar, _btnNavSquad, _btnNavLogs]);

        // Sidebar Footer Actions (Save & Backup Quick Buttons)
        var pnlSideActions = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 110,
            Padding = new Padding(12)
        };

        _btnSaveFile = new Button
        {
            Text = "💾 Salvar Alterações",
            Dock = DockStyle.Top,
            Height = 38,
            BackColor = Color.FromArgb(30, 130, 75),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Enabled = false,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
        };
        _btnSaveFile.FlatAppearance.BorderSize = 0;
        _btnSaveFile.Click += OnSaveFileClicked;

        _btnBackup = new Button
        {
            Text = "🛡 Criar Backup",
            Dock = DockStyle.Bottom,
            Height = 34,
            BackColor = ColorCard,
            ForeColor = ColorText,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Enabled = false
        };
        _btnBackup.FlatAppearance.BorderColor = ColorBorder;
        _btnBackup.Click += OnManualBackupClicked;

        pnlSideActions.Controls.AddRange([_btnSaveFile, _btnBackup]);

        _sidebarPanel.Controls.Add(pnlNav);
        _sidebarPanel.Controls.Add(pnlSideActions);
        _sidebarPanel.Controls.Add(pnlBrand);
    }

    private Button CreateNavButton(string text, int top)
    {
        var btn = new Button
        {
            Text = text,
            Top = top,
            Left = 0,
            Width = 204,
            Height = 42,
            FlatStyle = FlatStyle.Flat,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(14, 0, 0, 0),
            BackColor = ColorSidebar,
            ForeColor = ColorTextMuted,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
            Cursor = Cursors.Hand
        };
        btn.FlatAppearance.BorderSize = 0;
        btn.MouseEnter += (s, e) =>
        {
            if (btn.Tag as string != "active")
            {
                btn.BackColor = Color.FromArgb(32, 34, 52);
                btn.ForeColor = Color.White;
            }
        };
        btn.MouseLeave += (s, e) =>
        {
            if (btn.Tag as string != "active")
            {
                btn.BackColor = ColorSidebar;
                btn.ForeColor = ColorTextMuted;
            }
        };
        return btn;
    }

    private void SwitchView(Panel targetView, Button activeBtn)
    {
        _viewHome.Visible = (_viewHome == targetView);
        _viewFinance.Visible = (_viewFinance == targetView);
        _viewCalendar.Visible = (_viewCalendar == targetView);
        _viewSquad.Visible = (_viewSquad == targetView);
        _viewLogs.Visible = (_viewLogs == targetView);

        foreach (var btn in new[] { _btnNavHome, _btnNavFinance, _btnNavCalendar, _btnNavSquad, _btnNavLogs })
        {
            if (btn == activeBtn)
            {
                btn.Tag = "active";
                btn.BackColor = Color.FromArgb(35, 45, 75);
                btn.ForeColor = ColorAccent;
                btn.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            }
            else
            {
                btn.Tag = "inactive";
                btn.BackColor = ColorSidebar;
                btn.ForeColor = ColorTextMuted;
                btn.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);
            }
        }
    }

    private void BuildViews()
    {
        BuildHomeView();
        BuildFinanceView();
        BuildCalendarView();
        BuildSquadView();
        BuildLogsView();

        _contentPanel.Controls.AddRange([_viewHome, _viewFinance, _viewCalendar, _viewSquad, _viewLogs]);
    }

    // ==========================================
    // 1. HOME / SAVE SELECTION VIEW
    // ==========================================
    private void BuildHomeView()
    {
        _viewHome = new Panel { Dock = DockStyle.Fill, AutoScroll = true };

        var lblTitle = new Label
        {
            Text = "Carregar Save da Master Liga",
            Font = new Font("Segoe UI", 14f, FontStyle.Bold),
            ForeColor = Color.White,
            Location = new Point(10, 10),
            AutoSize = true
        };

        // Card 1: Auto Detection
        var cardAuto = new GroupBox
        {
            Text = "Modo Automático (Saves detectados na pasta do PES 2019)",
            Location = new Point(10, 48),
            Size = new Size(780, 130),
            ForeColor = ColorAccent,
            BackColor = ColorCard
        };

        _cboAutoSaves = new ComboBox
        {
            Location = new Point(20, 40),
            Size = new Size(520, 28),
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = ColorInputBg,
            ForeColor = ColorText
        };

        _btnLoadAutoSave = new Button
        {
            Text = "⚡ Carregar Selecionado",
            Location = new Point(555, 38),
            Size = new Size(200, 32),
            BackColor = Color.FromArgb(40, 90, 160),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        _btnLoadAutoSave.FlatAppearance.BorderSize = 0;
        _btnLoadAutoSave.Click += OnLoadAutoSaveClicked;

        _btnRefreshAutoSaves = new Button
        {
            Text = "🔄 Buscar Novamente",
            Location = new Point(20, 80),
            Size = new Size(160, 30),
            BackColor = ColorInputBg,
            ForeColor = ColorText,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        _btnRefreshAutoSaves.FlatAppearance.BorderColor = ColorBorder;
        _btnRefreshAutoSaves.Click += (s, e) => AutoDetectFolderAndPopulateSaves();

        cardAuto.Controls.AddRange([_cboAutoSaves, _btnLoadAutoSave, _btnRefreshAutoSaves]);

        // Card 2: Manual Selection
        var cardManual = new GroupBox
        {
            Text = "Modo Manual (Selecione qualquer arquivo ML* no computador)",
            Location = new Point(10, 195),
            Size = new Size(780, 110),
            ForeColor = ColorAccent,
            BackColor = ColorCard
        };

        _txtManualFilePath = new TextBox
        {
            Location = new Point(20, 40),
            Size = new Size(500, 28),
            BackColor = ColorInputBg,
            ForeColor = ColorText,
            PlaceholderText = "Cole o caminho completo ou selecione o arquivo ML..."
        };

        _btnBrowseManual = new Button
        {
            Text = "📁 Procurar...",
            Location = new Point(530, 38),
            Size = new Size(110, 32),
            BackColor = ColorInputBg,
            ForeColor = ColorText,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        _btnBrowseManual.FlatAppearance.BorderColor = ColorBorder;
        _btnBrowseManual.Click += OnBrowseManualClicked;

        _btnLoadManual = new Button
        {
            Text = "Abrir",
            Location = new Point(650, 38),
            Size = new Size(105, 32),
            BackColor = Color.FromArgb(40, 90, 160),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        _btnLoadManual.FlatAppearance.BorderSize = 0;
        _btnLoadManual.Click += OnLoadManualPathClicked;

        cardManual.Controls.AddRange([_txtManualFilePath, _btnBrowseManual, _btnLoadManual]);

        // Card 3: Loaded Save Details & Thumbnail
        var cardInfo = new GroupBox
        {
            Text = "Detalhes do Save Carregado",
            Location = new Point(10, 320),
            Size = new Size(780, 150),
            ForeColor = ColorAccent,
            BackColor = ColorCard
        };

        _picSaveLogo = new PictureBox
        {
            Location = new Point(20, 30),
            Size = new Size(100, 100),
            SizeMode = PictureBoxSizeMode.Zoom,
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = ColorBgDark
        };

        _lblFilePath = new Label
        {
            Text = "Arquivo: Nenhum save ativo.",
            Location = new Point(140, 35),
            Size = new Size(600, 24),
            Font = new Font("Segoe UI", 10f, FontStyle.Bold),
            ForeColor = Color.White
        };

        _lblStatus = new Label
        {
            Text = "Status: Aguardando seleção de arquivo de save.",
            Location = new Point(140, 68),
            Size = new Size(600, 24),
            ForeColor = ColorSuccess
        };

        var lblHelpNext = new Label
        {
            Text = "💡 Dica: Após carregar o save, utilize o menu lateral para gerenciar Finanças, Calendário e Elenco.",
            Location = new Point(140, 102),
            Size = new Size(620, 30),
            ForeColor = ColorTextMuted
        };

        cardInfo.Controls.AddRange([_picSaveLogo, _lblFilePath, _lblStatus, lblHelpNext]);

        _viewHome.Controls.AddRange([lblTitle, cardAuto, cardManual, cardInfo]);
    }

    // ==========================================
    // 2. FINANCE VIEW
    // ==========================================
    private void BuildFinanceView()
    {
        _viewFinance = new Panel { Dock = DockStyle.Fill, AutoScroll = true };

        var lblTitle = new Label
        {
            Text = "Gestão de Finanças do Clube",
            Font = new Font("Segoe UI", 14f, FontStyle.Bold),
            ForeColor = Color.White,
            Location = new Point(10, 10),
            AutoSize = true
        };

        // Search Group
        var grpSearch = new GroupBox
        {
            Text = "1. Localizador Automático / Manual de Finanças",
            Location = new Point(10, 48),
            Size = new Size(780, 160),
            ForeColor = ColorAccent,
            BackColor = ColorCard
        };

        var lblHelp = new Label
        {
            Text = "Abra seu PES, veja os valores em Sala do Treinador > Finanças > Saldo do Clube e digite:",
            Location = new Point(15, 25),
            AutoSize = true,
            ForeColor = ColorTextMuted
        };

        var lblCurTransfer = new Label { Text = "Orçamento de Transferência atual (€):", Location = new Point(15, 55), AutoSize = true, ForeColor = ColorText };
        _txtTransferCurrent = new TextBox { Location = new Point(15, 78), Size = new Size(220, 28), BackColor = ColorInputBg, ForeColor = ColorText };

        var lblCurSalary = new Label { Text = "Orçamento de Salários atual (€) [Opcional]:", Location = new Point(255, 55), AutoSize = true, ForeColor = ColorText };
        _txtSalaryCurrent = new TextBox { Location = new Point(255, 78), Size = new Size(220, 28), BackColor = ColorInputBg, ForeColor = ColorText };

        _btnSearchBudget = new Button
        {
            Text = "🔍 Localizar no Save",
            Location = new Point(495, 75),
            Size = new Size(250, 34),
            BackColor = Color.FromArgb(40, 90, 160),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        _btnSearchBudget.FlatAppearance.BorderSize = 0;
        _btnSearchBudget.Click += OnSearchBudgetClicked;

        var lblCandidate = new Label { Text = "Candidatos encontrados no save:", Location = new Point(15, 115), AutoSize = true, ForeColor = ColorTextMuted };
        _cboCandidates = new ComboBox
        {
            Location = new Point(200, 112),
            Size = new Size(545, 28),
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = ColorInputBg,
            ForeColor = ColorText
        };
        _cboCandidates.SelectedIndexChanged += OnCandidateSelected;

        grpSearch.Controls.AddRange([lblHelp, lblCurTransfer, _txtTransferCurrent, lblCurSalary, _txtSalaryCurrent, _btnSearchBudget, lblCandidate, _cboCandidates]);

        // Edit Group
        var grpEdit = new GroupBox
        {
            Text = "2. Editar Valores de Orçamento",
            Location = new Point(10, 220),
            Size = new Size(780, 175),
            ForeColor = ColorAccent,
            BackColor = ColorCard
        };

        var lblNewTrans = new Label { Text = "Novo Orçamento de Transferências (€):", Location = new Point(15, 30), AutoSize = true, ForeColor = ColorText };
        _numNewTransfer = new NumericUpDown
        {
            Location = new Point(15, 55),
            Size = new Size(240, 28),
            Maximum = 1_999_999_999,
            ThousandsSeparator = true,
            BackColor = ColorInputBg,
            ForeColor = ColorText
        };

        var lblNewSal = new Label { Text = "Novo Orçamento de Salários (€):", Location = new Point(275, 30), AutoSize = true, ForeColor = ColorText };
        _numNewSalary = new NumericUpDown
        {
            Location = new Point(275, 55),
            Size = new Size(240, 28),
            Maximum = 1_999_999_999,
            ThousandsSeparator = true,
            BackColor = ColorInputBg,
            ForeColor = ColorText
        };

        _btnApplyBudget = new Button
        {
            Text = "✔ Aplicar Alterações no Save",
            Location = new Point(535, 52),
            Size = new Size(220, 34),
            BackColor = Color.FromArgb(30, 130, 75),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Enabled = false,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
        };
        _btnApplyBudget.FlatAppearance.BorderSize = 0;
        _btnApplyBudget.Click += OnApplyBudgetClicked;

        // Presets
        var btnAdd50M = new Button { Text = "+ €50M Transf", Size = new Size(115, 30), Location = new Point(15, 105), BackColor = ColorInputBg, ForeColor = ColorText, FlatStyle = FlatStyle.Flat };
        btnAdd50M.FlatAppearance.BorderColor = ColorBorder;
        btnAdd50M.Click += (s, e) => _numNewTransfer.Value = Math.Min(_numNewTransfer.Maximum, _numNewTransfer.Value + 50_000_000);

        var btnAdd100M = new Button { Text = "+ €100M Transf", Size = new Size(115, 30), Location = new Point(140, 105), BackColor = ColorInputBg, ForeColor = ColorText, FlatStyle = FlatStyle.Flat };
        btnAdd100M.FlatAppearance.BorderColor = ColorBorder;
        btnAdd100M.Click += (s, e) => _numNewTransfer.Value = Math.Min(_numNewTransfer.Maximum, _numNewTransfer.Value + 100_000_000);

        var btnAdd10MSal = new Button { Text = "+ €10M Salário", Size = new Size(115, 30), Location = new Point(275, 105), BackColor = ColorInputBg, ForeColor = ColorText, FlatStyle = FlatStyle.Flat };
        btnAdd10MSal.FlatAppearance.BorderColor = ColorBorder;
        btnAdd10MSal.Click += (s, e) => _numNewSalary.Value = Math.Min(_numNewSalary.Maximum, _numNewSalary.Value + 10_000_000);

        var btnAdd25MSal = new Button { Text = "+ €25M Salário", Size = new Size(115, 30), Location = new Point(400, 105), BackColor = ColorInputBg, ForeColor = ColorText, FlatStyle = FlatStyle.Flat };
        btnAdd25MSal.FlatAppearance.BorderColor = ColorBorder;
        btnAdd25MSal.Click += (s, e) => _numNewSalary.Value = Math.Min(_numNewSalary.Maximum, _numNewSalary.Value + 25_000_000);

        var btnMaxAll = new Button { Text = "⭐ Max Finanças (€500M / €100M)", Size = new Size(220, 30), Location = new Point(535, 105), BackColor = Color.FromArgb(70, 50, 110), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        btnMaxAll.FlatAppearance.BorderSize = 0;
        btnMaxAll.Click += (s, e) =>
        {
            _numNewTransfer.Value = 500_000_000;
            _numNewSalary.Value = 100_000_000;
        };

        grpEdit.Controls.AddRange([lblNewTrans, _numNewTransfer, lblNewSal, _numNewSalary, _btnApplyBudget, btnAdd50M, btnAdd100M, btnAdd10MSal, btnAdd25MSal, btnMaxAll]);

        _viewFinance.Controls.AddRange([lblTitle, grpSearch, grpEdit]);
    }

    // ==========================================
    // 3. CALENDAR VIEW
    // ==========================================
    private void BuildCalendarView()
    {
        _viewCalendar = new Panel { Dock = DockStyle.Fill, AutoScroll = true };

        var lblTitle = new Label
        {
            Text = "Calendário & Simulação de Datas",
            Font = new Font("Segoe UI", 14f, FontStyle.Bold),
            ForeColor = Color.White,
            Location = new Point(10, 10),
            AutoSize = true
        };

        var grpCal = new GroupBox
        {
            Text = "Avançar e Definir Data da Carreira",
            Location = new Point(10, 48),
            Size = new Size(780, 220),
            ForeColor = ColorAccent,
            BackColor = ColorCard
        };

        var lblCurDateTitle = new Label { Text = "Data atual da carreira:", Location = new Point(20, 35), AutoSize = true, ForeColor = ColorText };
        _lblCurrentCareerDate = new Label
        {
            Text = "--/--/----",
            Location = new Point(175, 33),
            AutoSize = true,
            Font = new Font("Segoe UI", 12f, FontStyle.Bold),
            ForeColor = ColorAccent
        };

        var lblNewDateTitle = new Label { Text = "Definir nova data:", Location = new Point(20, 80), AutoSize = true, ForeColor = ColorText };
        _dtpCareerDate = new DateTimePicker
        {
            Location = new Point(155, 77),
            Size = new Size(160, 28),
            Format = DateTimePickerFormat.Short,
            Enabled = false,
            CalendarMonthBackground = ColorInputBg,
            CalendarForeColor = ColorText
        };

        _btnApplyDate = new Button
        {
            Text = "📅 Atualizar Data",
            Location = new Point(330, 75),
            Size = new Size(170, 32),
            BackColor = Color.FromArgb(40, 90, 160),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Enabled = false
        };
        _btnApplyDate.FlatAppearance.BorderSize = 0;
        _btnApplyDate.Click += OnApplyDateClicked;

        var lblQuickActions = new Label { Text = "Atalhos rápidos de avanço no tempo:", Location = new Point(20, 125), AutoSize = true, ForeColor = ColorTextMuted };

        _btnAdvanceWeek = new Button { Text = "+ 1 Semana", Location = new Point(20, 150), Size = new Size(130, 32), BackColor = ColorInputBg, ForeColor = ColorText, FlatStyle = FlatStyle.Flat, Enabled = false, Cursor = Cursors.Hand };
        _btnAdvanceWeek.FlatAppearance.BorderColor = ColorBorder;
        _btnAdvanceWeek.Click += (s, e) => _dtpCareerDate.Value = _dtpCareerDate.Value.AddDays(7);

        _btnAdvanceMonth = new Button { Text = "+ 1 Mês", Location = new Point(160, 150), Size = new Size(120, 32), BackColor = ColorInputBg, ForeColor = ColorText, FlatStyle = FlatStyle.Flat, Enabled = false, Cursor = Cursors.Hand };
        _btnAdvanceMonth.FlatAppearance.BorderColor = ColorBorder;
        _btnAdvanceMonth.Click += (s, e) => _dtpCareerDate.Value = _dtpCareerDate.Value.AddMonths(1);

        _btnJumpTransferEnd = new Button { Text = "⏳ Fim Janela Verão (31/08)", Location = new Point(290, 150), Size = new Size(210, 32), BackColor = ColorInputBg, ForeColor = ColorText, FlatStyle = FlatStyle.Flat, Enabled = false, Cursor = Cursors.Hand };
        _btnJumpTransferEnd.FlatAppearance.BorderColor = ColorBorder;
        _btnJumpTransferEnd.Click += (s, e) => _dtpCareerDate.Value = new DateTime(_dtpCareerDate.Value.Year, 8, 31);

        grpCal.Controls.AddRange([lblCurDateTitle, _lblCurrentCareerDate, lblNewDateTitle, _dtpCareerDate, _btnApplyDate, lblQuickActions, _btnAdvanceWeek, _btnAdvanceMonth, _btnJumpTransferEnd]);

        _viewCalendar.Controls.AddRange([lblTitle, grpCal]);
    }

    // ==========================================
    // 4. SQUAD MANAGEMENT VIEW (v0.3)
    // ==========================================
    private void BuildSquadView()
    {
        _viewSquad = new Panel { Dock = DockStyle.Fill, AutoScroll = true };

        var lblTitle = new Label
        {
            Text = "Gestão do Elenco, Contratos & Espírito de Equipe",
            Font = new Font("Segoe UI", 14f, FontStyle.Bold),
            ForeColor = Color.White,
            Location = new Point(10, 10),
            AutoSize = true
        };

        // Top Box: Team Spirit Control
        var grpSpirit = new GroupBox
        {
            Text = "Espírito de Equipe (Team Spirit / Entrosamento)",
            Location = new Point(10, 48),
            Size = new Size(780, 75),
            ForeColor = ColorAccent,
            BackColor = ColorCard
        };

        var lblSpirit = new Label { Text = "Nível de Espírito de Equipe (1..99):", Location = new Point(15, 30), AutoSize = true, ForeColor = ColorText };
        _numTeamSpirit = new NumericUpDown
        {
            Location = new Point(245, 27),
            Size = new Size(80, 28),
            Minimum = 1,
            Maximum = 99,
            Value = 94,
            BackColor = ColorInputBg,
            ForeColor = ColorText
        };

        _btnApplyTeamSpirit = new Button
        {
            Text = "⭐ Aplicar Espírito de Equipe",
            Location = new Point(340, 25),
            Size = new Size(220, 32),
            BackColor = Color.FromArgb(40, 90, 160),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        _btnApplyTeamSpirit.FlatAppearance.BorderSize = 0;
        _btnApplyTeamSpirit.Click += OnApplyTeamSpiritClicked;

        var btnMaxSpirit = new Button
        {
            Text = "Máximo (99)",
            Location = new Point(570, 25),
            Size = new Size(110, 32),
            BackColor = ColorInputBg,
            ForeColor = ColorText,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnMaxSpirit.FlatAppearance.BorderColor = ColorBorder;
        btnMaxSpirit.Click += (s, e) => _numTeamSpirit.Value = 99;

        grpSpirit.Controls.AddRange([lblSpirit, _numTeamSpirit, _btnApplyTeamSpirit, btnMaxSpirit]);

        // Middle: Squad DataGrid
        var grpSquadTable = new GroupBox
        {
            Text = "Jogadores do Elenco Ativo",
            Location = new Point(10, 130),
            Size = new Size(780, 360),
            ForeColor = ColorAccent,
            BackColor = ColorCard
        };

        _lblSquadCount = new Label
        {
            Text = "Nenhum jogador carregado.",
            Location = new Point(15, 25),
            AutoSize = true,
            ForeColor = ColorTextMuted
        };

        _gridSquad = new DataGridView
        {
            Location = new Point(15, 50),
            Size = new Size(750, 300),
            BackgroundColor = ColorBgDark,
            ForeColor = ColorText,
            GridColor = ColorBorder,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            MultiSelect = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            RowHeadersVisible = false,
            BorderStyle = BorderStyle.None,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };
        _gridSquad.ColumnHeadersDefaultCellStyle.BackColor = ColorSidebar;
        _gridSquad.ColumnHeadersDefaultCellStyle.ForeColor = ColorAccent;
        _gridSquad.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
        _gridSquad.DefaultCellStyle.BackColor = ColorCard;
        _gridSquad.DefaultCellStyle.ForeColor = ColorText;
        _gridSquad.DefaultCellStyle.SelectionBackColor = Color.FromArgb(50, 75, 120);
        _gridSquad.DefaultCellStyle.SelectionForeColor = Color.White;
        _gridSquad.SelectionChanged += OnSquadPlayerSelected;

        grpSquadTable.Controls.AddRange([_lblSquadCount, _gridSquad]);

        // Bottom: Selected Player Editor
        var grpPlayerEdit = new GroupBox
        {
            Text = "Editar Salário do Jogador Selecionado",
            Location = new Point(10, 498),
            Size = new Size(780, 85),
            ForeColor = ColorAccent,
            BackColor = ColorCard
        };

        _lblSelectedPlayerInfo = new Label
        {
            Text = "Selecione um jogador na tabela acima.",
            Location = new Point(15, 25),
            Size = new Size(300, 20),
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            ForeColor = Color.White
        };

        var lblSalaryVal = new Label { Text = "Salário Base Scaled:", Location = new Point(15, 50), AutoSize = true, ForeColor = ColorTextMuted };
        _numPlayerSalaryScaled = new NumericUpDown
        {
            Location = new Point(140, 47),
            Size = new Size(110, 26),
            Maximum = 100000,
            BackColor = ColorInputBg,
            ForeColor = ColorText
        };
        _numPlayerSalaryScaled.ValueChanged += (s, e) =>
        {
            long est = (long)_numPlayerSalaryScaled.Value * 12200L;
            _lblPlayerSalaryEurEst.Text = $"≈ € {est:N0} / ano";
        };

        _lblPlayerSalaryEurEst = new Label
        {
            Text = "≈ € 0 / ano",
            Location = new Point(260, 50),
            Size = new Size(200, 20),
            ForeColor = ColorSuccess,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
        };

        _btnApplyPlayerSalary = new Button
        {
            Text = "✔ Salvar Salário do Jogador",
            Location = new Point(480, 44),
            Size = new Size(280, 32),
            BackColor = Color.FromArgb(30, 130, 75),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Enabled = false,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
        };
        _btnApplyPlayerSalary.FlatAppearance.BorderSize = 0;
        _btnApplyPlayerSalary.Click += OnApplyPlayerSalaryClicked;

        grpPlayerEdit.Controls.AddRange([_lblSelectedPlayerInfo, lblSalaryVal, _numPlayerSalaryScaled, _lblPlayerSalaryEurEst, _btnApplyPlayerSalary]);

        _viewSquad.Controls.AddRange([lblTitle, grpSpirit, grpSquadTable, grpPlayerEdit]);
    }

    // ==========================================
    // 5. LOGS VIEW
    // ==========================================
    private void BuildLogsView()
    {
        _viewLogs = new Panel { Dock = DockStyle.Fill, AutoScroll = true };

        var lblTitle = new Label
        {
            Text = "Registro de Atividades & Terminal",
            Font = new Font("Segoe UI", 14f, FontStyle.Bold),
            ForeColor = Color.White,
            Location = new Point(10, 10),
            AutoSize = true
        };

        var grpLog = new GroupBox
        {
            Text = "Console de Operações",
            Location = new Point(10, 48),
            Size = new Size(780, 520),
            ForeColor = ColorAccent,
            BackColor = ColorCard
        };

        _txtLog = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            Font = new Font("Consolas", 9.5f),
            BackColor = ColorBgDark,
            ForeColor = Color.FromArgb(200, 225, 255),
            BorderStyle = BorderStyle.None
        };

        grpLog.Controls.Add(_txtLog);
        _viewLogs.Controls.AddRange([lblTitle, grpLog]);
    }

    // ==========================================
    // LOGIC & EVENT HANDLERS
    // ==========================================
    private void AutoDetectFolderAndPopulateSaves()
    {
        _cboAutoSaves.Items.Clear();
        _detectedSaveDirectory = SaveFileManager.TryDetectSaveDirectory();

        if (_detectedSaveDirectory != null)
        {
            Log($"Pasta padrão do PES 2019 detectada: {_detectedSaveDirectory}");

            // Try load player names from EDIT00000000 if available
            string editPath = Path.Combine(_detectedSaveDirectory, "EDIT00000000");
            if (File.Exists(editPath))
            {
                try
                {
                    var editDesc = Pes19Crypto.Decrypt(File.ReadAllBytes(editPath), Pes19Crypto.MasterKeyPes19);
                    _editPlayerNames = MlSquadService.LoadPlayerNamesFromEdit(editDesc.Data);
                    Log($"Banco de nomes EDIT00000000 carregado com sucesso: {_editPlayerNames.Count} jogadores mapeados.");
                }
                catch (Exception ex)
                {
                    Log($"Aviso ao ler EDIT00000000: {ex.Message}");
                }
            }

            var saves = SaveFileManager.GetMasterLeagueSaves(_detectedSaveDirectory);
            if (saves.Length > 0)
            {
                foreach (var savePath in saves)
                {
                    _cboAutoSaves.Items.Add(new SaveItem(savePath));
                }
                _cboAutoSaves.SelectedIndex = 0;
                _btnLoadAutoSave.Enabled = true;
                Log($"Saves de Master Liga encontrados automaticamente: {saves.Length}");
            }
            else
            {
                _cboAutoSaves.Items.Add("Nenhum arquivo ML* encontrado na pasta padrão");
                _cboAutoSaves.SelectedIndex = 0;
                _btnLoadAutoSave.Enabled = false;
                Log("Nenhum arquivo ML* encontrado na pasta padrão.");
            }
        }
        else
        {
            _cboAutoSaves.Items.Add("Pasta KONAMI não encontrada automaticamente");
            _cboAutoSaves.SelectedIndex = 0;
            _btnLoadAutoSave.Enabled = false;
            Log("Pasta KONAMI não localizada em Documentos. Utilize a busca manual.");
        }
    }

    private void Log(string message)
    {
        string timestamp = DateTime.Now.ToString("HH:mm:ss");
        _txtLog.AppendText($"[{timestamp}] {message}{Environment.NewLine}");
    }

    private void LoadSaveFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            MessageBox.Show($"O arquivo especificado não existe:\n{filePath}", "Arquivo não encontrado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            _currentFilePath = filePath;
            _txtManualFilePath.Text = filePath;
            _currentSave = SaveFileManager.LoadSave(_currentFilePath);

            string fileName = Path.GetFileName(_currentFilePath);
            _lblFilePath.Text = $"Arquivo: {fileName}";
            _lblActiveSaveIndicator.Text = $"● {fileName}";
            _lblActiveSaveIndicator.ForeColor = ColorSuccess;
            _lblStatus.Text = $"Save Carregado com Sucesso! (Payload: {_currentSave.Data.Length:N0} bytes)";
            _btnSaveFile.Enabled = true;
            _btnBackup.Enabled = true;

            Log($"Save carregado com sucesso: {_currentFilePath}");
            Log($"Tipo: {_currentSave.FileHeader.FileTypeString} | Payload: {_currentSave.Data.Length:N0} bytes");

            // Render logo
            if (_currentSave.Logo.Length > 0)
            {
                try
                {
                    using var ms = new MemoryStream(_currentSave.Logo);
                    _picSaveLogo.Image = Image.FromStream(ms);
                }
                catch
                {
                    _picSaveLogo.Image = null;
                }
            }
            else
            {
                _picSaveLogo.Image = null;
            }

            // Populate Calendar Date
            var careerDate = MlCalendarService.ReadDate(_currentSave.Data);
            if (careerDate.HasValue)
            {
                _lblCurrentCareerDate.Text = careerDate.Value.ToString("dd/MM/yyyy");
                _dtpCareerDate.Value = careerDate.Value;
                _dtpCareerDate.Enabled = true;
                _btnApplyDate.Enabled = true;
                _btnAdvanceWeek.Enabled = true;
                _btnAdvanceMonth.Enabled = true;
                _btnJumpTransferEnd.Enabled = true;
                Log($"Data do calendário detectada: {careerDate.Value:dd/MM/yyyy}");
            }

            // Populate Finances
            _cboCandidates.Items.Clear();
            if (_currentSave.Data.Length > MlFinanceService.DefaultBalanceOffset + 4)
            {
                int currentTransRaw = BinaryPrimitives.ReadInt32LittleEndian(_currentSave.Data.AsSpan(MlFinanceService.DefaultTransferOffset, 4));
                int currentBalRaw = BinaryPrimitives.ReadInt32LittleEndian(_currentSave.Data.AsSpan(MlFinanceService.DefaultBalanceOffset, 4));

                if (currentTransRaw > 0)
                {
                    long transEur = (long)currentTransRaw * 100;
                    long balEur = (long)currentBalRaw * 100;

                    _txtTransferCurrent.Text = transEur.ToString();
                    _txtSalaryCurrent.Text = balEur.ToString();

                    var candidate = new FinanceCandidate
                    {
                        Offset = MlFinanceService.DefaultTransferOffset,
                        FoundValue = transEur,
                        SecondValueOffset = MlFinanceService.DefaultBalanceOffset,
                        SecondValue = balEur,
                        ProximityToSecondValue = 16
                    };

                    _cboCandidates.Items.Add(candidate);
                    _cboCandidates.SelectedIndex = 0;
                    _btnApplyBudget.Enabled = true;
                    Log($"Finanças pré-carregadas: Transferência = {transEur:N0} €, Saldo = {balEur:N0} €");
                }
            }

            // Populate Squad (v0.3)
            PopulateSquad();

            // Populate Team Spirit
            byte teamSpirit = MlSquadService.ReadTeamSpirit(_currentSave.Data);
            if (teamSpirit > 0)
            {
                _numTeamSpirit.Value = Math.Clamp(teamSpirit, (byte)1, (byte)99);
                Log($"Espírito de Equipe (Team Spirit) lido: {teamSpirit}");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao descriptografar save: {ex.Message}", "Erro de Leitura", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Log($"Falha ao abrir save: {ex.Message}");
        }
    }

    private void PopulateSquad()
    {
        if (_currentSave == null) return;

        _currentSquad = MlSquadService.ReadSquad(_currentSave.Data, _editPlayerNames);
        _lblSquadCount.Text = $"Total de jogadores no elenco: {_currentSquad.Count}";

        // Configure DataGridView
        _gridSquad.DataSource = null;
        _gridSquad.Columns.Clear();
        _gridSquad.AutoGenerateColumns = false;

        _gridSquad.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "SlotIndex", HeaderText = "Slot", Width = 60 });
        _gridSquad.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "PlayerId", HeaderText = "ID", Width = 80 });
        _gridSquad.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Name", HeaderText = "Nome do Jogador", FillWeight = 160 });
        _gridSquad.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "SalaryScaled", HeaderText = "Salário (Base)", Width = 110 });
        _gridSquad.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "SalaryEurEstimated",
            HeaderText = "Salário Anual (€)",
            DefaultCellStyle = new DataGridViewCellStyle { Format = "C0", FormatProvider = new System.Globalization.CultureInfo("pt-PT") },
            Width = 140
        });

        _gridSquad.DataSource = _currentSquad;
        Log($"Elenco do clube carregado com sucesso: {_currentSquad.Count} jogadores identificados.");
    }

    private void OnSquadPlayerSelected(object? sender, EventArgs e)
    {
        if (_gridSquad.CurrentRow?.DataBoundItem is MlPlayerEntry player)
        {
            _lblSelectedPlayerInfo.Text = $"{player.Name} (ID: {player.PlayerId} | Slot: {player.SlotIndex})";
            _numPlayerSalaryScaled.Value = Math.Clamp(player.SalaryScaled, 0, (int)_numPlayerSalaryScaled.Maximum);
            long est = (long)player.SalaryScaled * 12200L;
            _lblPlayerSalaryEurEst.Text = $"≈ € {est:N0} / ano";
            _btnApplyPlayerSalary.Enabled = true;
        }
        else
        {
            _lblSelectedPlayerInfo.Text = "Selecione um jogador na tabela acima.";
            _btnApplyPlayerSalary.Enabled = false;
        }
    }

    private void OnApplyPlayerSalaryClicked(object? sender, EventArgs e)
    {
        if (_currentSave == null || _gridSquad.CurrentRow?.DataBoundItem is not MlPlayerEntry player)
            return;

        int newSalaryScaled = (int)_numPlayerSalaryScaled.Value;
        bool ok = MlSquadService.WritePlayerSalary(_currentSave.Data, player.SaveOffset, newSalaryScaled);
        if (ok)
        {
            player.SalaryScaled = newSalaryScaled;
            player.SalaryEurEstimated = (long)newSalaryScaled * 12200L;
            _gridSquad.Refresh();
            Log($"Salário do jogador {player.Name} atualizado para base {newSalaryScaled} (≈ € {player.SalaryEurEstimated:N0}/ano) na memória.");
            MessageBox.Show($"Salário do jogador {player.Name} atualizado na memória!\nClique em 'Salvar Alterações' para gravar o arquivo ML.", "Salário Atualizado", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void OnApplyTeamSpiritClicked(object? sender, EventArgs e)
    {
        if (_currentSave == null) return;

        byte spirit = (byte)_numTeamSpirit.Value;
        bool ok = MlSquadService.WriteTeamSpirit(_currentSave.Data, spirit);
        if (ok)
        {
            Log($"Espírito de Equipe alterado para {spirit} na memória.");
            MessageBox.Show($"Espírito de Equipe atualizado para {spirit}!\nClique em 'Salvar Alterações' para gravar no arquivo.", "Espírito de Equipe Atualizado", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void OnApplyDateClicked(object? sender, EventArgs e)
    {
        if (_currentSave == null) return;

        DateTime selectedDate = _dtpCareerDate.Value.Date;
        bool ok = MlCalendarService.WriteDate(_currentSave, selectedDate);
        if (ok)
        {
            _lblCurrentCareerDate.Text = selectedDate.ToString("dd/MM/yyyy");
            Log($"Nova data aplicada na memória: {selectedDate:dd/MM/yyyy}.");
            MessageBox.Show($"Data da carreira atualizada para {selectedDate:dd/MM/yyyy}!\nClique em 'Salvar Alterações' para gravar no arquivo.", "Data Atualizada", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void OnLoadAutoSaveClicked(object? sender, EventArgs e)
    {
        if (_cboAutoSaves.SelectedItem is SaveItem item)
        {
            LoadSaveFile(item.FullPath);
        }
    }

    private void OnBrowseManualClicked(object? sender, EventArgs e)
    {
        using var ofd = new OpenFileDialog
        {
            Title = "Selecione manualmente o arquivo de save da Master Liga",
            Filter = "Saves da Master Liga (ML*)|ML*|Todos os Arquivos (*.*)|*.*",
            InitialDirectory = _detectedSaveDirectory ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        };

        if (ofd.ShowDialog() == DialogResult.OK)
        {
            LoadSaveFile(ofd.FileName);
        }
    }

    private void OnLoadManualPathClicked(object? sender, EventArgs e)
    {
        string path = _txtManualFilePath.Text.Trim('"', ' ', '\t');
        if (string.IsNullOrWhiteSpace(path))
        {
            MessageBox.Show("Cole ou digite o caminho completo do arquivo de save.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        LoadSaveFile(path);
    }

    private void OnSearchBudgetClicked(object? sender, EventArgs e)
    {
        if (_currentSave == null)
        {
            MessageBox.Show("Abra um arquivo de save antes de buscar os valores.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!long.TryParse(_txtTransferCurrent.Text.Trim().Replace(".", "").Replace(",", ""), out long transVal))
        {
            MessageBox.Show("Informe o valor atual do Orçamento de Transferências exibido no PES.", "Valor Inválido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        long? salVal = null;
        if (long.TryParse(_txtSalaryCurrent.Text.Trim().Replace(".", "").Replace(",", ""), out long parsedSal))
        {
            salVal = parsedSal;
        }

        Log($"Pesquisando no save por Transferência = {transVal:N0} €" + (salVal.HasValue ? $" e Salários = {salVal:N0} €..." : "..."));

        var candidates = MlFinanceService.FindCandidates(_currentSave.Data, transVal, salVal);
        _cboCandidates.Items.Clear();

        if (candidates.Count == 0)
        {
            Log("Nenhuma ocorrência encontrada com esse valor exato.");
            MessageBox.Show("Nenhum bloco financeiro encontrado com esse valor exato.", "Não encontrado", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        foreach (var c in candidates)
        {
            _cboCandidates.Items.Add(c);
        }

        _cboCandidates.SelectedIndex = 0;
        Log($"{candidates.Count} candidato(s) localizado(s) no buffer.");
    }

    private void OnCandidateSelected(object? sender, EventArgs e)
    {
        if (_cboCandidates.SelectedItem is FinanceCandidate candidate && _currentSave != null)
        {
            _selectedCandidate = candidate;
            _numNewTransfer.Value = candidate.FoundValue;

            if (candidate.SecondValueOffset >= 0)
            {
                _numNewSalary.Value = candidate.SecondValue;
            }
            else
            {
                try
                {
                    int nearby = MlFinanceService.ReadInt32(_currentSave.Data, candidate.Offset + 8);
                    _numNewSalary.Value = Math.Max(0, nearby);
                }
                catch
                {
                    _numNewSalary.Value = 0;
                }
            }

            _btnApplyBudget.Enabled = true;
            Log($"Candidato selecionado: Offset 0x{candidate.Offset:X8}.");
        }
    }

    private void OnApplyBudgetClicked(object? sender, EventArgs e)
    {
        if (_currentSave == null || _selectedCandidate == null) return;

        long newTrans = (long)_numNewTransfer.Value;
        long newSal = (long)_numNewSalary.Value;

        MlFinanceService.WriteScaledBudget(_currentSave.Data, _selectedCandidate.Offset, newTrans);
        if (_selectedCandidate.SecondValueOffset >= 0)
        {
            MlFinanceService.WriteScaledBudget(_currentSave.Data, _selectedCandidate.SecondValueOffset, newSal);
        }

        Log($"Alterações aplicadas na memória: Transferência = {newTrans:N0} €, Saldo/Teto = {newSal:N0} €.");
        MessageBox.Show("Valores atualizados na memória com sucesso!\nClique em 'Salvar Alterações' para gravar o arquivo ML.", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void OnSaveFileClicked(object? sender, EventArgs e)
    {
        if (_currentSave == null || string.IsNullOrEmpty(_currentFilePath)) return;

        try
        {
            string savedPath = SaveFileManager.SaveWithBackup(_currentFilePath, _currentSave, createBackup: true);
            Log($"Save re-criptografado e salvo com sucesso em: {savedPath}");
            Log($"Backup de segurança criado automaticamente.");
            MessageBox.Show("Save salvo com sucesso!\n\nUm backup de segurança foi criado antes de salvar.", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao salvar arquivo: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Log($"Erro ao salvar: {ex.Message}");
        }
    }

    private void OnManualBackupClicked(object? sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_currentFilePath) || !File.Exists(_currentFilePath))
        {
            MessageBox.Show("Nenhum save carregado para fazer backup.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            string dir = Path.GetDirectoryName(_currentFilePath)!;
            string fileName = Path.GetFileName(_currentFilePath);
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string backupPath = Path.Combine(dir, $"{fileName}.manual_bak_{timestamp}");
            File.Copy(_currentFilePath, backupPath, overwrite: true);
            Log($"Backup manual criado: {backupPath}");
            MessageBox.Show($"Backup manual criado:\n{backupPath}", "Backup Criado", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao criar backup manual: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private sealed class SaveItem(string fullPath)
    {
        public string FullPath { get; } = fullPath;

        public override string ToString()
        {
            var info = new FileInfo(FullPath);
            return $"{info.Name}  ({info.Length / 1024:N0} KB - {info.LastWriteTime:dd/MM/yyyy HH:mm})";
        }
    }
}
