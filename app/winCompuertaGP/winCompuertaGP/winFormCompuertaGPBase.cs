using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
//using preFacturaTeamPediatrico.DAL;
using winCompuertaGP.BLL;
using System.Text.RegularExpressions;
using System.Xml;
using System.Linq;
using Comun;
using IntegradorDeGP;

namespace winCompuertaGP
{
    public partial class winFormCompuertaGPBase : Form
    {
        private Main mainController;
        //private int idPrefacturaSeleccionada;
        private ParametrosDB configuracion;
        private object[] idDetallePrefacturaSeleccionada;

        private object celdaActual;
        private List<int> filasActualizadas;

        DateTime fechaIni = DateTime.Today;
        DateTime fechaFin = DateTime.Today.AddHours(23).AddMinutes(59).AddSeconds(59);

        int dePeriodo = DateTime.Now.Year * 100 + 01;
        int aPeriodo = DateTime.Now.Year * 100 + DateTime.Now.Month;

        //bool filtrarDetallePrefactura;

        public winFormCompuertaGPBase()
        {
            try
            {
                InitializeComponent();

                //dgvFacturas.AutoGenerateColumns = false;

                cmbBEstado.SelectedIndex = 0;


                mainController = new Main("");
                mainController.eventoErrorDB += MainController_eventoErrorDB;
                configuracion = new ParametrosDB();
                cargarEmpresas();
                
                var empresaDefault = configuracion.Empresas.Where(x => x.Idbd == configuracion.DefaultDB).First();
                cmbBxCompannia.SelectedIndex = configuracion.Empresas.IndexOf(empresaDefault);

                celdaActual = null;
                filasActualizadas = new List<int>();

                lblFecha.Text = DateTime.Now.ToShortDateString();
                lblUsuario.Text = Environment.UserName;

                //this.idPrefacturaSeleccionada = -1;
                this.idDetallePrefacturaSeleccionada = new object[2];
                //this.filtrarDetallePrefactura = false;
            }
            catch (Exception exc)
            {
                txtbxMensajes.Text = string.Concat( exc.Message, Environment.NewLine, exc?.InnerException?.ToString(), Environment.NewLine, exc.StackTrace , Environment.NewLine);
            }

        }

        private void MainController_eventoErrorDB(object sender, ErrorEventArgs e)
        {
            txtbxMensajes.Text += e.mensajeError + Environment.NewLine;
        }

        private void cargarEmpresas()
        {
            try
            {
                cmbBxCompannia.Items.Clear();
                foreach (Empresa e in configuracion.Empresas)
                {
                    cmbBxCompannia.Items.Add(e.Idbd + "->" + e.NombreBd);
                }
            }
            catch (Exception exc)
            {
                txtbxMensajes.AppendText(exc.Message + Environment.NewLine);
            }
        }

        private void cmbBxCompannia_SelectedIndexChanged(object sender, EventArgs e)
        {
            cargarDatosEmpresa(((ComboBox)sender).SelectedIndex);
        }

        private void winformPreFactura_Load(object sender, EventArgs e)
        {

        }

        private void cargarDatosEmpresa(int index)
        {

            // Limpiar los DataGridViews
            dgvFacturas.Rows.Clear();
            //dgvDetallesPrefactura.Rows.Clear();
            //dgvPrestaciones.Rows.Clear();

            // Limpiar los filtros y las cabeceras
            limpiarFiltrosPreFacturas();

            // Establecer el nuevo string de conexión
            //mainController.connectionString = this.connections[index];
            configuracion.GetParametros(index);
            mainController.connectionString = configuracion.ConnStringSourceEFUI;

            // Limpiar los mensajes
            txtbxMensajes.Text = "";

            // Verificar la conexión
            if (mainController.probarConexion())
            {
                ActualizarStatus();
                // Recargar los datos del grid
                filtrarPreFacturas();
            }
            else
                txtbxMensajes.Text = "Contacte al administrador. No se pudo establecer la conexión para la compañía seleccionada. [cargarDatosEmpresa]";
        }


        #region Utiles UI
        private void reportaProgreso(int i, string s)
        {
            //iProgreso = i;
            tsProgressBar1.Increment(i);
            //tsProgressBar1.Refresh();

            if (tsProgressBar1.Value == tsProgressBar1.Maximum)
                tsProgressBar1.Value = 0;

            txtbxMensajes.AppendText(s + "\r\n");
            txtbxMensajes.Refresh();
        }

        private void reportaAlertas(string s)
        {
            textBoxAlertas.AppendText(s + "\r\n");
            textBoxAlertas.Refresh();
        }

        private void salirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        
        #endregion

        #region Búsqueda UI
        private void btnBuscar_Click(object sender, EventArgs e)
        {
            try
            {
                txtbxMensajes.Text = "";
                var errores = validarFiltrosPreFacturas();
                if (errores == "")
                {
                    var c = filtrarPreFacturas();
                    txtbxMensajes.Text = "";
                    txtbxMensajes.AppendText("Total de documentos encontrados: " + c + Environment.NewLine);
                }
                else
                    txtbxMensajes.Text = errores;
            }
            catch (Exception exc)
            {
                txtbxMensajes.Text = string.Concat(exc.Message, Environment.NewLine, exc?.InnerException.ToString(), Environment.NewLine);
            }
        }

        private void hoytsMenuItem4_Click(object sender, EventArgs e)
        {
            fechaIni = DateTime.Today;
            fechaFin = DateTime.Today.AddHours(23).AddMinutes(59).AddSeconds(59);
            checkBoxFecha.Checked = false;
            tsDropDownFiltro.Text = hoytsMenuItem4.Text;
            filtrarPreFacturas();
        }

        private void ayertsMenuItem5_Click(object sender, EventArgs e)
        {
            fechaIni = DateTime.Today.AddDays(-1);
            fechaFin = DateTime.Today.AddDays(-1).AddHours(23).AddMinutes(59).AddSeconds(59);
            checkBoxFecha.Checked = false;
            tsDropDownFiltro.Text = ayertsMenuItem5.Text;
            filtrarPreFacturas();
        }

        private void ultimos7tsMenuItem6_Click(object sender, EventArgs e)
        {
            fechaIni = DateTime.Today.AddDays(-6);
            fechaFin = DateTime.Today.AddHours(23).AddMinutes(59).AddSeconds(59);
            checkBoxFecha.Checked = false;
            tsDropDownFiltro.Text = ultimos7tsMenuItem6.Text;
            filtrarPreFacturas();
        }

        private void ultimos30tsMenuItem7_Click(object sender, EventArgs e)
        {
            fechaIni = DateTime.Today.AddDays(-29);
            fechaFin = DateTime.Today.AddHours(23).AddMinutes(59).AddSeconds(59);
            checkBoxFecha.Checked = false;
            tsDropDownFiltro.Text = ultimos30tsMenuItem7.Text;
            filtrarPreFacturas();
        }

        private void ultimos60tsMenuItem8_Click(object sender, EventArgs e)
        {
            fechaIni = DateTime.Today.AddDays(-59);
            fechaFin = DateTime.Today.AddHours(23).AddMinutes(59).AddSeconds(59);
            checkBoxFecha.Checked = false;
            tsDropDownFiltro.Text = ultimos60tsMenuItem8.Text;
            filtrarPreFacturas();
        }

        private void mesActualtsMenuItem9_Click(object sender, EventArgs e)
        {
            fechaIni = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            fechaFin = fechaIni.AddMonths(1);
            int ultimoDia = fechaFin.Day;
            fechaFin = fechaFin.AddDays(-ultimoDia);
            checkBoxFecha.Checked = false;
            tsDropDownFiltro.Text = mesActualtsMenuItem9.Text;
            filtrarPreFacturas();
        }

        private void tsDropDownFiltro_TextChanged(object sender, EventArgs e)
        {
            txtbxMensajes.Text = "";
        }

        private int filtrarPreFacturas()
        {
            bool cbFechaMarcada = checkBoxFecha.Checked;
            DateTime fini = dtPickerDesde.Value.Date.AddHours(0).AddMinutes(0).AddSeconds(0);
            DateTime ffin = dtPickerHasta.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
            if (!checkBoxPacientes_numero_pf.Checked && !checkBoxFecha.Checked && !checkBoxEstado.Checked && !checkBoxPacientes_nombre_cliente.Checked && !checkBoxPacientes_referencia.Checked && !checkBoxPacientes_sopnumbe.Checked)
            {
                cbFechaMarcada = true;
                fini = fechaIni;
                ffin = fechaFin;
            }

            var datos = mainController.getPrefacturas(
                                                        checkBoxPacientes_numero_pf.Checked,
                                                        textBoxPacientes_numero_pf_desde.Text,
                                                        textBoxPacientes_numero_pf_hasta.Text,
                                                        cbFechaMarcada,
                                                        fini,
                                                        ffin,
                                                        checkBoxEstado.Checked,
                                                        cmbBEstado.SelectedItem.ToString(),
                                                        checkBoxPacientes_nombre_cliente.Checked,
                                                        textBoxPacientes_nombre_cliente.Text,
                                                        checkBoxPacientes_referencia.Checked,
                                                        textBoxPacientes_referencia.Text,
                                                        checkBoxPacientes_sopnumbe.Checked,
                                                        textBoxPacientes_sopnumbe_desde.Text,
                                                        textBoxPacientes_sopnumbe_hasta.Text
                                                    );
            bindingSource1.DataSource = datos;
            dgvFacturas.AutoGenerateColumns = false;
            dgvFacturas.DataSource = bindingSource1;
            dgvFacturas.AutoResizeColumns();
            //dgvFacturas.RowHeadersVisible = false;

            return datos.Count;
        }

        // Valida los campos de filtrado y devuelve los errores
        private string validarFiltrosPreFacturas()
        {
            string errores = "";
            if (checkBoxFecha.Checked)
            {
                if (dtPickerDesde.Value > dtPickerHasta.Value)
                    errores += "El campo fecha inicial debe ser menor que la fecha final." + Environment.NewLine;
            }

            return errores;
        }

        private void limpiarFiltrosPreFacturas()
        {
            checkBoxPacientes_numero_pf.Checked = false;
            checkBoxPacientes_nombre_cliente.Checked = false;
            checkBoxFecha.Checked = false;
            checkBoxEstado.Checked = false;
            checkBoxPacientes_sopnumbe.Checked = false;
            checkBoxPacientes_referencia.Checked = false;

            textBoxPacientes_numero_pf_desde.Text = "";
            textBoxPacientes_numero_pf_hasta.Text = "";
            textBoxPacientes_nombre_cliente.Text = "";
            dtPickerDesde.ResetText();
            dtPickerHasta.ResetText();
            cmbBEstado.SelectedIndex = 0;
            textBoxPacientes_sopnumbe_desde.Text = "";
            textBoxPacientes_sopnumbe_hasta.Text = "";
            textBoxPacientes_referencia.Text = "";
        }

        private void toolStripButton12_Click(object sender, EventArgs e)
        {
            marcarDesmarcarTodo(dgvFacturas);
        }

        // Marca/desmarca todos los checks del grid especificado
        private void marcarDesmarcarTodo(DataGridView dgv)
        {
            bool value = false;
            bool flag = false;

            foreach (DataGridViewRow item in dgv.Rows)
            {
                if (!flag)
                {
                    value = !(Boolean.Parse(dgv.Rows[0].Cells[0].Value.ToString()));
                    flag = true;
                }
                item.Cells[0].Value = value;

                // Console.WriteLine(((DataGridViewCheckBoxCell)item.Cells[0]));
            }
            dgv.Refresh();
        }


        private void dgvPacientes_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            //var row = e.RowIndex;
            //// validar que el doble click no sea en la cabecera
            //if (row != -1)
            //{
            //    try
            //    {
            //        int numeroPF = Convert.ToInt32(((DataGridView)sender).Rows[row].Cells[1].Value);
            //        limpiarFiltrosDetallesPrefacturas();
            //        var prefactura = mainController.findPrefactura(numeroPF);
            //        if (prefactura != null)
            //        {
            //            this.idPrefacturaSeleccionada = prefactura.NUMERO_PF;
            //            mostrarDetallesPrefactura(prefactura);
            //        }
            //    }
            //    catch (Exception exc)
            //    {
            //        txtbxMensajes.Text = exc.Message + "\r\n";
            //    }
            //}
        }

        #endregion

        #region Integración de documentos

        private void tsBtnIntegraFactura_Click(object sender, EventArgs e)
        {
            try
            {
                //tsProgressBar1.Style = ProgressBarStyle.Marquee;
                //tsProgressBar1.MarqueeAnimationSpeed = 40;

                IntegraVentasBandejaDB bandejaDB = new IntegraVentasBandejaDB(configuracion, Environment.UserName);

                bandejaDB.eventoProgreso += reportaProgreso;   // BandejaDB_eventoProgreso;

                bandejaDB.ProcesaBandejaDB("LISTO", "ENVIAR_A_GP");

                filtrarPreFacturas();


            }
            catch (Exception ex)
            {
                reportaProgreso(0, ex.Message);
            }
            finally
            {
                //tsProgressBar1.Style = ProgressBarStyle.Continuous;
                //tsProgressBar1.MarqueeAnimationSpeed = 0;
            }

        }

        #endregion

        #region Otros
        private void tabControlPrefactura_SelectedIndexChanged(object sender, EventArgs e)
        {
            //txtbxMensajes.Text = "";

            //if (mainController.probarConexion())
            //{
            //    try
            //    {
            //        var prefacturaSeleccionada = mainController.findPrefactura(this.idPrefacturaSeleccionada);
            //        if (prefacturaSeleccionada == null)
            //        {
            //            dgvDetallesPrefactura.Rows.Clear();
            //            limpiarCabecerasDetallesPrefacturas();

            //            // deshabilitar la creación de detalles cuando no hay prefactura seleccionada
            //            toolStripButton6.Enabled = false;
            //        }
            //        else
            //        {
            //            // habilitar la creación de detalles cuando hay prefactura seleccionada
            //            toolStripButton6.Enabled = true;
            //        }

            //        var detallePrefactura = mainController.findDetallePrefactura(this.idDetallePrefacturaSeleccionada);
            //        if (detallePrefactura == null)
            //        {
            //            limpiarCabecerasPrestaciones();
            //        }
            //    }
            //    catch (Exception exc)
            //    {
            //        txtbxMensajes.Text = exc.Message + "\r\n";
            //    }
            //}
            //else
            //{
            //    txtbxMensajes.Text = "No se pudo establecer la conexión con el servidor." + "\r\n";
            //}
        }

        private void tabControlPreFactura_Selecting(object sender, TabControlCancelEventArgs e)
        {
            //e.Cancel = comprobarCambiosDetallesPrefactura() || comprobarCambiosPrestaciones();

            //if (comprobarCambiosDetallesPrefactura() || comprobarCambiosPrestaciones())
            //    txtbxMensajes.Text = "Para cambiar de pestaña debe guardar los cambios realizados." + "\r\n";
        }

        private void toolStripButton13_Click(object sender, EventArgs e)
        {
            var filas = getFilasSeleccionadas(dgvFacturas);
            //if (filas.Count > 0)
            //{
            //    var dialogResult = MessageBox.Show(this, "¿Confirma que desea eliminar los elementos seleccionados?", "Confirmación", MessageBoxButtons.OKCancel);
            //    if (dialogResult == DialogResult.OK)
            //    {
            //        txtbxMensajes.Text = "";

            //        var c = 0;
            //        foreach (DataGridViewRow item in filas)
            //        {
            //            var id = Convert.ToInt32(item.Cells[1].Value);
            //            var prefactura = mainController.findPrefactura(id);
            //            if (prefactura != null)
            //            {
            //                if (prefactura.STATUS.Equals("BORRADOR"))
            //                {
            //                    try
            //                    {
            //                        mainController.removePrefactura(id);
            //                        c++;
            //                    }
            //                    catch (Exception exc)
            //                    {
            //                        txtbxMensajes.AppendText("No se pudo eliminar la prefactura con número " + prefactura.NUMERO_PF + "\r\n");
            //                        txtbxMensajes.AppendText(exc.Message + "\r\n");
            //                    }
            //                }
            //                else
            //                {
            //                    txtbxMensajes.AppendText("No se pudo eliminar la prefactura con número " + prefactura.NUMERO_PF + "; el estado no es BORRADOR.\r\n");
            //                }
            //            }
            //        }

            //        if (c > 0)
            //        {
            //            var cant = filtrarPacientes();

            //            txtbxMensajes.AppendText("Prefacturas eliminadas satisfactoriamente: " + c + "\r\n");
            //            txtbxMensajes.AppendText("Total de prefacturas existentes: " + cant + "\r\n");
            //        }
            //    }
            //}
            //else
            //{
            //    MessageBox.Show(this, "No existen elementos seleccionados.", "Información");
            //}
        }

        private void dgvPacientes_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dgvFacturas.IsCurrentCellDirty)
            {
                dgvFacturas.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        // Devuelve las filas seleccionadas del grid especificado
        private List<DataGridViewRow> getFilasSeleccionadas(DataGridView dgv)
        {
            var listado = new List<DataGridViewRow>();
            foreach (DataGridViewRow item in dgv.Rows)
            {
                if (Boolean.Parse(item.Cells[0].Value.ToString()))
                    listado.Add(item);
            }

            return listado;
        }

        #endregion
        
        #region Actualizar/Cambiar status de documentos
        private void ActualizarStatus()
        {
            try
            {
                IntegraVentasBandejaDB bandejaDB = new IntegraVentasBandejaDB(configuracion, Environment.UserName);

                bandejaDB.eventoProgreso += reportaProgreso;
                //Si la factura fue contabilizada en GP cambia el status de INTEGRADO a CONTABILIZADO
                bandejaDB.ProcesaBandejaDBActualizaStatus("INTEGRADO", "CONTABILIZA_FACTURA_EN_GP");

            }
            catch (Exception ex)
            {
                reportaProgreso(0, ex.Message);
            }

        }

        private void tsbActualizarStatus_Click(object sender, EventArgs e)
        {
            ActualizarStatus();
            filtrarPreFacturas();
        }


        void ActualizarStatus(int idLog, string docStatus, string transicion)
        {
            configuracion.GetParametros(cmbBxCompannia.SelectedIndex);
            IntegraVentasBandejaDB IntegraSOP = new IntegraVentasBandejaDB(configuracion, Environment.UserName);
            IntegraSOP.eventoProgreso += new IntegraVentasBandejaDB.LogHandler(reportaProgreso);
            IntegraSOP.ProcesaBandejaDB(idLog, docStatus, transicion);

        }

        private void tsMenuItemCambiarAListo_Click(object sender, EventArgs e)
        {
            txtbxMensajes.Text = "";
            txtbxMensajes.Refresh();

            if (dgvFacturas.RowCount == 0)
            {
                txtbxMensajes.Text = "No hay documentos para procesar. Verifique los criterios de búsqueda.";
            }
            else
                try
                {
                    string idLog = dgvFacturas.SelectedRows[0].Cells[1].Value.ToString();
                    string docStatus = dgvFacturas.SelectedRows[0].Cells[8].Value.ToString();

                    this.ActualizarStatus(int.Parse(idLog), docStatus, "ELIMINA_FACTURA_EN_GP");
                    filtrarPreFacturas();
                    reportaProgreso(0, "Proceso finalizado.");
                }
                catch (Exception gr)
                {

                    reportaProgreso(0, gr.Message);
                }

        }

        private void anuleDespuesDeContabilizadaTsMenuItem_Click(object sender, EventArgs e)
        {
            txtbxMensajes.Text = "";
            txtbxMensajes.Refresh();

            if (dgvFacturas.RowCount == 0)
            {
                txtbxMensajes.Text = "No hay documentos para procesar. Verifique los criterios de búsqueda.";
            }
            else
                try
                {
                    string idLog = dgvFacturas.SelectedRows[0].Cells[1].Value.ToString();
                    string docStatus = dgvFacturas.SelectedRows[0].Cells[8].Value.ToString();

                    this.ActualizarStatus(int.Parse(idLog), docStatus, "ANULA_FACTURA_RM_EN_GP");
                    filtrarPreFacturas();
                    reportaProgreso(0, "Proceso finalizado.");
                }
                catch (Exception gr)
                {

                    reportaProgreso(0, gr.Message);
                }
        }

        #endregion
    }
}
