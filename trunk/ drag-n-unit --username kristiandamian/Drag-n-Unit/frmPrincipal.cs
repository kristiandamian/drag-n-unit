using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
//agregar estos al add-in vs
using System.Reflection;
using System.IO;
using System.Xml;

namespace Drag_n_Unit
{
    public partial class frmPrincipal : Form
    {
        private const string ArchivoDeDatos="Info.xml";
        private Ensamblados cEnsamblados;
        private CElementosNUnit cElementosNunit;
        private string[] strElementosInvalidos;
        public frmPrincipal()
        {
            InitializeComponent();
        }

        private void frmPrincipal_Load(object sender, EventArgs e)
        {
            if (ExisteArchivoDatos())
            {
                toolTipText.SetToolTip(treeElementos, "Arrastre un elemento (ya sea constructor, propiedad o metodo) para crear el codigo correspondiente");
                toolTipText.SetToolTip(btnAbrir, "Seleccione un assembly para cargarlo en el TreeView");
                toolTipText.SetToolTip(txtDescripcion, "Breve descripción del elemento de NUnit");
                cEnsamblados = new Ensamblados();
                cElementosNunit = new CElementosNUnit();
                strElementosInvalidos = new string[1];
                LeoXML();
            }
            else
            {
                MessageBox.Show("No se encuentra el archivo con las definiciones de los elementos de NUnit, por favor verifique", "ATENCION", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        private bool ExisteArchivoDatos()
        {
            return File.Exists(Application.StartupPath + "//" + ArchivoDeDatos);
        }
        #region "manejo de tree del assembly"
        private void btnAbrir_Click(object sender, EventArgs e)
        {
            try
            {
                openFile.ShowDialog();
                string RutaCompleta = openFile.FileName;
                if (RutaCompleta.Length > 0)
                {
                    string[] NombreFile = RutaCompleta.Split('\\');
                    Ensamblado Ass = new Ensamblado(RutaCompleta);
                    lblAssembly.Text += " " + Ass.NombreArchivo.Trim();
                    cEnsamblados.Add(Ass);
                    LlenoTree(Ass);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void LlenoTree(Ensamblado Ensamb)
        {
            //Nodo principal
            TreeNode tnPadre;            
            tnPadre = treeElementos.Nodes.Add(Ensamb.NombreArchivo);
            foreach (Type t in Ensamb.Tipos)
            {
                try
                {
                    //Nodo derivado, uno por cada clase
                    
                    TreeNode tn;
                    tn = tnPadre.Nodes.Add(t.FullName);
                    //obtengo los constructores
                    ConstructorInfo[] ctrInfo = t.GetConstructors();                    
                    TreeNode tn2;
                    tn2 = tn.Nodes.Add("Constructores");
                    //foreach (ConstructorInfo c in ctrInfo)
                    {
                        string[] NombreConstructor = t.FullName.Split('.');
                        tn2.Nodes.Add(NombreConstructor[NombreConstructor.Length-1]);
                    }
                    //obtengo las propiedades
                    try
                    {            
                        PropertyInfo[] pInfo = t.GetProperties();
                        tn2 = tn.Nodes.Add("Propiedades");

                        foreach (PropertyInfo p in pInfo)
                        {
                            tn2.Nodes.Add(p.ToString());
                        }
                    }
                    catch (Exception e)
                    {
                        tn2 = tn.Nodes.Add("Propiedades");
                        tn2.Nodes.Add(e.ToString());
                    }
                    //Obtengo los Metodos
                    try
                    {
                        MethodInfo[] mInfo = t.GetMethods();                        
                        tn2 = tn.Nodes.Add("Metodos");                        
                        foreach (MethodInfo m in mInfo)
                        {
                            tn2.Nodes.Add(m.ToString());
                        }

                    }
                    catch (Exception e)
                    {
                        tn2 = tn.Nodes.Add("Metodos");
                        tn2.Nodes.Add(e.ToString());
                    }

                    /*/Obtengo Eventos
                    try
                    {
                        EventInfo[] eInfo = t.GetEvents();
                        tn2 = tn.Nodes.Add("Eventos");

                        foreach (EventInfo e in eInfo)
                        {
                            tn2.Nodes.Add(e.Name);
                        }

                    }
                    catch (Exception e)
                    {
                        tn2 = tn.Nodes.Add("Eventos");
                        tn2.Nodes.Add(e.ToString());
                    }*/
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                }
            }
        }
        /// <summary>
        /// Inicio a arrastrar un elemento
        /// </summary>
        private void treeElementos_ItemDrag(object sender, ItemDragEventArgs e)
        {
            try
            {
                if(ElementoValido(treeElementos.SelectedNode.Parent.Text))
                {
                    int iTipo=-1;
                    switch (treeElementos.SelectedNode.Parent.Text)
                    {
                        case "Constructores":
                            iTipo = (int)Ensamblado.Tipo.Contructores;
                            break;
                        case "Propiedades":
                            iTipo = (int)Ensamblado.Tipo.Propiedades;
                            break;
                        case "Metodos":
                            iTipo = (int)Ensamblado.Tipo.Metodos;
                            break;
                        case "Eventos":
                            iTipo = (int)Ensamblado.Tipo.Eventos;
                            break;
                        default:
                            break;
                    }
                    if (iTipo >= 0)
                    {
                        string sTexto=cEnsamblados[cEnsamblados.Find(treeElementos.SelectedNode.Text)].Definicion(treeElementos.SelectedNode.Text,(Ensamblado.Tipo) iTipo);
                        this.DoDragDrop(sTexto, DragDropEffects.All);
                    }
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Valido si el padre es un constructor, propiedad, metodo o evento, lo que significa que es un nodo hijo de ultimo nivel
        /// </summary>
        /// <param name="Texto">Nombre del nodo padre del que queremos arrastrar</param>
        /// <returns>true si es hijo de uno es los mencionados anteriormente</returns>
        private bool ElementoValido(string Texto)
        {
            bool bRetorno = false;
            switch (Texto)
            {
                case "Constructores":
                case "Propiedades":                                        
                case "Metodos":
                case "Eventos":
                    bRetorno = true;
                    break;
                default:
                    break;
            }
            return bRetorno;
        }
        #endregion
        #region "manejo de XML"
        private void LeoXML()
        {
            XmlTextReader xmlLector = new XmlTextReader(ArchivoDeDatos);
            try
            {
                string strElemento="";
                int NumElementosInvalidos = 0;
                //elementos para el constructor del objeto
                string sRoot = "";
                string sNombre = "";
                string sDesc = "";
                string sCodigo = "";                

                xmlLector.Read();
                xmlLector.MoveToElement();                
                TreeNode Nodo = new TreeNode();
                TreeNode NodoHijo = new TreeNode();
                treeNUnit.Nodes.Clear();
                
                while (xmlLector.Read())
                {
                    ///Es un elemento, no comentario ni encabezado
                    if (xmlLector.NodeType == XmlNodeType.Element)
                    {
                        strElemento=xmlLector.LocalName;
                    }
                    if (xmlLector.NodeType == XmlNodeType.Text)
                    {
                        switch (strElemento)
                        {
                            case "Marco":
                                {
                                    string texto=LeoXmlEncriptado(xmlLector.Value);
                                    strElementosInvalidos[NumElementosInvalidos] = texto;
                                    Nodo=treeNUnit.Nodes.Add(texto);
                                    NumElementosInvalidos++;
                                    strElementosInvalidos = Libreria.Arreglos.redim(strElementosInvalidos, strElementosInvalidos.Length+1);
                                } break;
                            case "TipoRoot": 
                                {
                                    string texto = LeoXmlEncriptado(xmlLector.Value);
                                    strElementosInvalidos[NumElementosInvalidos] = texto;
                                    NodoHijo=AgregoNodo(ref Nodo, texto);
                                    NumElementosInvalidos++;
                                    strElementosInvalidos = Libreria.Arreglos.redim(strElementosInvalidos, strElementosInvalidos.Length + 1);

                                } break;
                            case "Root":
                                {
                                    sRoot = LeoXmlEncriptado(xmlLector.Value);
                                } break;
                            case "Nombre":
                                {
                                    sNombre = LeoXmlEncriptado(xmlLector.Value);
                                    AgregoNodo(ref NodoHijo, sNombre);
                                } break;
                            case "Desc":
                                {
                                    sDesc = LeoXmlEncriptado(xmlLector.Value);
                                } break;
                            case "Codigo":
                                {
                                    sCodigo = LeoXmlEncriptado(xmlLector.Value);
                                    ElementoNUnit elemento=new ElementoNUnit(sRoot,sNombre,sDesc,sCodigo);
                                    cElementosNunit.Add(elemento);
                                } break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al abrir el archivo XML" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        /// <summary>
        /// Leo un elemento que esta encriptado y pos lo desencripto
        /// </summary>
        /// <param name="NodoEcriptado">Texto encriptado</param>
        /// <returns>texto desencriptado</returns>
        private string LeoXmlEncriptado(string NodoEcriptado)
        {
            string sRetorno;
            byte[] _bytEncriptado = Convert.FromBase64String(NodoEcriptado);
            sRetorno = EncriptacionAsimetrica.MiRijndael.Desencriptar(_bytEncriptado);
            return sRetorno;
        }

        private TreeNode AgregoNodo(ref TreeNode Nodo, string Texto)
        {
            TreeNode NodoReturn=null;
            if (treeNUnit.Nodes.Count == 0)
            {
                Nodo = treeNUnit.Nodes.Add(Texto, Texto, 2, 2);
            }
            else
            {
                NodoReturn=Nodo.Nodes.Add(Texto, Texto, 3);            
                Nodo.Expand();
            }
            return NodoReturn;
        }
        
        #endregion

        #region "manejo drag del treeview del NUnit"
        private void treeNUnit_ItemDrag(object sender, ItemDragEventArgs e)
        {
            try
            {
                if (!ElementoInvalido(treeNUnit.SelectedNode.Text))
                {
                    this.DoDragDrop(treeNUnit.SelectedNode.Text, DragDropEffects.All);   
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
        }

        private bool ElementoInvalido(string Texto)
        {
            bool bRetorno = false;
            foreach (string str in strElementosInvalidos)
            {
                if (Texto == str)//no lo quiero arrastrar
                {
                    bRetorno = true;
                    break;
                }
            }
            return bRetorno;
        }
        #endregion

        private void treeNUnit_AfterSelect(object sender, TreeViewEventArgs e)
        {
            ElementoNUnit elemento= cElementosNunit.Find(treeNUnit.SelectedNode.Text);
            if (elemento == null)//no ta en la coleccion
            {
                txtCodigo.Text = "";
                txtDescripcion.Text = "";
            }
            else
            {
                txtCodigo.Text = elemento.Codigo;
                txtDescripcion.Text = elemento.Descripcion;                
            }
        }
    }
}
