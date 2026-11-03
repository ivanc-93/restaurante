#nullable enable
namespace Restaurante.Frontend.Formularios;
partial class FormularioCatalogos
{
    private System.ComponentModel.IContainer? components;
    private ComboBox cbModo = null!;
    private DataGridView dgvRegistros = null!;
    private TextBox ctNombre = null!;
    private TextBox ctTelefono = null!;
    private TextBox ctCorreo = null!;
    private TextBox ctCategoria = null!;
    private TextBox ctUbicacion = null!;
    private ComboBox cbTipo = null!;
    private NumericUpDown nudPrecio = null!;
    private CheckBox chkAlcoholica = null!;
    private Button btnNuevo = null!;
    private Button btnEditar = null!;
    private Button btnGuardar = null!;
    private Button btnDesactivar = null!;
    private FlowLayoutPanel flpCampos = null!;
    private TableLayoutPanel tlpLayout = null!;
    private Label lblNombreLabel = null!;
    private Label lblTelefonoLabel = null!;
    private Label lblCorreoLabel = null!;
    private Label lblCategoriaLabel = null!;
    private Label lblUbicacionLabel = null!;
    private Label lblTipoLabel = null!;
    private Label lblPrecioLabel = null!;
    private TextBox ctBuscarCatalogo = null!;
    private Label lblTituloCatalogo = null!, lblEstadoEdicion = null!;
    protected override void Dispose(bool disposing)
    {
        if (disposing)
            components?.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        cbModo = new ComboBox()
        {
            Name = "cbModo"
        };
        dgvRegistros = new DataGridView()
        {
            Name = "dgvRegistros"
        };
        ctNombre = new TextBox()
        {
            Name = "ctNombre"
        };
        ctTelefono = new TextBox()
        {
            Name = "ctTelefono"
        };
        ctCorreo = new TextBox()
        {
            Name = "ctCorreo"
        };
        ctCategoria = new TextBox()
        {
            Name = "ctCategoria"
        };
        ctUbicacion = new TextBox()
        {
            Name = "ctUbicacion"
        };
        cbTipo = new ComboBox()
        {
            Name = "cbTipo"
        };
        nudPrecio = new NumericUpDown()
        {
            Name = "nudPrecio"
        };
        chkAlcoholica = new CheckBox()
        {
            Name = "chkAlcoholica"
        };
        btnNuevo = new Button()
        {
            Name = "btnNuevo"
        };
        btnEditar = new Button()
        {
            Name = "btnEditar"
        };
        btnGuardar = new Button()
        {
            Name = "btnGuardar"
        };
        btnDesactivar = new Button()
        {
            Name = "btnDesactivar"
        };
        flpCampos = new FlowLayoutPanel()
        {
            Name = "flpCampos"
        };
        tlpLayout = new TableLayoutPanel()
        {
            Name = "tlpLayout"
        };
        lblNombreLabel = new Label()
        {
            Name = "lblNombreLabel"
        };
        lblTelefonoLabel = new Label()
        {
            Name = "lblTelefonoLabel"
        };
        lblCorreoLabel = new Label()
        {
            Name = "lblCorreoLabel"
        };
        lblCategoriaLabel = new Label()
        {
            Name = "lblCategoriaLabel"
        };
        lblUbicacionLabel = new Label()
        {
            Name = "lblUbicacionLabel"
        };
        lblTipoLabel = new Label()
        {
            Name = "lblTipoLabel"
        };
        lblPrecioLabel = new Label()
        {
            Name = "lblPrecioLabel"
        };
        ((System.ComponentModel.ISupportInitialize)dgvRegistros).BeginInit();
        ((System.ComponentModel.ISupportInitialize)nudPrecio).BeginInit();
        SuspendLayout();
        Font = Componentes.Fuente;
        AutoScaleDimensions = new SizeF(96, 96);
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = Componentes.Fondo;
        tlpLayout.Dock = DockStyle.Fill;
        tlpLayout.Padding = new Padding(15);
        tlpLayout.ColumnCount = 2;
        tlpLayout.RowCount = 2;
        tlpLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F));
        tlpLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
        tlpLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 86F));
        tlpLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        cbModo.DropDownStyle = ComboBoxStyle.DropDownList;
        cbModo.Items.AddRange(new object[] { "Clientes", "Productos" });
        cbModo.Width = 220;
        cbModo.SelectedIndexChanged += Modo_Cambio;
        dgvRegistros.Dock = DockStyle.Fill;
        dgvRegistros.ReadOnly = true;
        dgvRegistros.AllowUserToAddRows = false;
        dgvRegistros.AutoGenerateColumns = true;
        flpCampos.Dock = DockStyle.Fill;
        flpCampos.FlowDirection = FlowDirection.TopDown;
        flpCampos.WrapContents = false;
        flpCampos.AutoScroll = true;
        flpCampos.BackColor = Color.White;
        flpCampos.Padding = new Padding(18);
        flpCampos.Margin = new Padding(18, 0, 0, 0);
        lblEstadoEdicion = new Label
        {
            Text = "Nuevo registro",
            AutoSize = true,
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 16),
            Name = "lblEstadoEdicion"
        };
        lblNombreLabel.AutoSize = true;
        lblTelefonoLabel.AutoSize = true;
        lblCorreoLabel.AutoSize = true;
        lblCategoriaLabel.AutoSize = true;
        lblUbicacionLabel.AutoSize = true;
        lblTipoLabel.AutoSize = true;
        lblPrecioLabel.AutoSize = true;
        ctNombre.Width = 260;
        ctNombre.MaxLength = 120;
        lblNombreLabel.Text = "Nombre (obligatorio)";
        ctTelefono.Width = 260;
        ctTelefono.MaxLength = 30;
        lblTelefonoLabel.Text = "Teléfono del cliente";
        ctCorreo.Width = 260;
        ctCorreo.MaxLength = 150;
        lblCorreoLabel.Text = "Correo del cliente";
        ctCategoria.Width = 260;
        ctCategoria.MaxLength = 60;
        lblCategoriaLabel.Text = "Categoría del producto";
        ctUbicacion.Width = 260;
        ctUbicacion.MaxLength = 250;
        lblUbicacionLabel.Text = "Ubicación del restaurante";
        cbTipo.Width = 260;
        cbTipo.DropDownStyle = ComboBoxStyle.DropDownList;
        cbTipo.Items.AddRange(new object[] { "Plato", "Bebida" });
        lblTipoLabel.Text = "Tipo de producto";
        nudPrecio.Width = 260;
        nudPrecio.DecimalPlaces = 2;
        nudPrecio.Maximum = 99999999.99M;
        lblPrecioLabel.Text = "Precio (cero = cortesía)";
        chkAlcoholica.Text = "Bebida alcohólica";
        chkAlcoholica.AutoSize = true;
        btnNuevo.Text = "Nuevo / Limpiar";
        btnNuevo.Size = new Size(260, 48);
        btnNuevo.Click += Nuevo_Click;
        btnEditar.Text = "Editar seleccionado";
        btnEditar.Size = new Size(260, 32);
        btnEditar.Click += Seleccionar_Click;
        btnGuardar.Text = "Guardar cambios";
        btnGuardar.Size = new Size(260, 48);
        btnGuardar.Click += Guardar_Click;
        btnDesactivar.Text = "Desactivar seleccionado";
        btnDesactivar.Size = new Size(260, 32);
        btnDesactivar.Click += Desactivar_Click;
        foreach (var boton in new[]
        {
            btnNuevo,
            btnEditar,
            btnGuardar,
            btnDesactivar
        }

        )
        {
            Componentes.EstilizarBoton(boton, boton == btnGuardar);
            boton.AutoSize = false;
            boton.Height = 48;
            boton.Margin = new Padding(0, 10, 0, 0);
        }

        btnDesactivar.ForeColor = Color.FromArgb(151, 45, 43);
        Iconos.Aplicar(btnDesactivar, "user-minus");
        Iconos.Aplicar(btnGuardar, "save");
        Iconos.Aplicar(btnNuevo, "plus");
        Iconos.Aplicar(btnEditar, "pencil");
        flpCampos.Controls.AddRange(new Control[] { lblEstadoEdicion, lblNombreLabel, ctNombre, lblTelefonoLabel, ctTelefono, lblCorreoLabel, ctCorreo, lblCategoriaLabel, ctCategoria, lblTipoLabel, cbTipo, lblPrecioLabel, nudPrecio, chkAlcoholica, lblUbicacionLabel, ctUbicacion, btnGuardar, btnNuevo, btnEditar, btnDesactivar });
        lblTituloCatalogo = new Label
        {
            Text = "Clientes y productos",
            AutoSize = true,
            Font = new Font("Segoe UI", 24, FontStyle.Bold),
            Margin = new Padding(0, 0, 24, 0),
            Name = "lblTituloCatalogo"
        };
        var flpCabecera = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            WrapContents = true,
            Name = "flpCabecera"
        };
        flpCabecera.Controls.Add(lblTituloCatalogo);
        flpCabecera.Controls.Add(cbModo);
        ctBuscarCatalogo = new TextBox
        {
            Name = "buscarCatalogo",
            Dock = DockStyle.Fill,
            PlaceholderText = "Buscar por nombre, teléfono o correo",
            AccessibleName = "Buscar en el catálogo",
            Margin = new Padding(0, 0, 0, 12)
        };
        ctBuscarCatalogo.TextChanged += (_, _) => AplicarFiltro();
        var tlpListado = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Name = "tlpListado"
        };
        tlpListado.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        tlpListado.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        tlpListado.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        tlpListado.Controls.Add(ctBuscarCatalogo, 0, 0);
        tlpListado.Controls.Add(dgvRegistros, 0, 1);
        dgvRegistros.CellDoubleClick += (_, _) => Seleccionar_Click(this, EventArgs.Empty);
        dgvRegistros.DataBindingComplete += (_, _) =>
        {
            if (dgvRegistros.Columns.Contains("Id"))
                dgvRegistros.Columns["Id"].Visible = false;
            if (dgvRegistros.Columns.Contains("Telefono"))
                dgvRegistros.Columns["Telefono"].HeaderText = "Teléfono";
            foreach (DataGridViewRow fila in dgvRegistros.Rows)
                fila.MinimumHeight = 46;
        };
        tlpLayout.Controls.Add(flpCabecera, 0, 0);
        tlpLayout.SetColumnSpan(flpCabecera, 2);
        tlpLayout.Controls.Add(tlpListado, 0, 1);
        tlpLayout.Controls.Add(flpCampos, 1, 1);
        flpCampos.SizeChanged += (_, _) =>
        {
            var ancho = Math.Max(140, flpCampos.ClientSize.Width - flpCampos.Padding.Horizontal - SystemInformation.VerticalScrollBarWidth - 8);
            foreach (Control campo in flpCampos.Controls)
            {
                if (campo is Label or CheckBox)
                    campo.MaximumSize = new Size(ancho, 0);
                else
                    campo.Width = ancho;
            }
        };
        Controls.Add(tlpLayout);
        ClientSize = new Size(1180, 720);
        MinimumSize = new Size(850, 520);
        StartPosition = FormStartPosition.CenterParent;
        Text = "Clientes, productos y restaurante";
        ((System.ComponentModel.ISupportInitialize)dgvRegistros).EndInit();
        ((System.ComponentModel.ISupportInitialize)nudPrecio).EndInit();
        ResumeLayout(false);
        FormatoPresentacion.Aplicar(this);
    }
}
