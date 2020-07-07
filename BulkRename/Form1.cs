using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ArchestrA.GRAccess;

namespace BulkRename
{
    public partial class Form1 : Form
    {
        GRAccessApp grAccess = new GRAccessAppClass();
        IGalaxies galaxies;
        IGalaxy galaxy;
        ICommandResult cmdResult;
        IgObjects GalaxyObjects;
        ITemplate template;
        IInstance instance;

        List<MyObject> myObjects = new List<MyObject>();

        bool _loggedIn = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Help help = new Help();
            help.ShowDialog();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                label2.Enabled = true;
                label3.Enabled = true;

                textBox1.Enabled = true;
                textBox2.Enabled = true;
            }
            else
            {
                label2.Enabled = false;
                label3.Enabled = false;

                textBox1.Enabled = false;
                textBox2.Enabled = false;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            galaxies = grAccess.QueryGalaxies();
            foreach (IGalaxy _galaxy in galaxies)
            {
                galaxyList.Items.Add(_galaxy.Name);
            }
        }

        private void galaxyList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (galaxyList.SelectedIndex >= 0)
            {
                btnConnect.Enabled = true;
            }
            else
            {
                btnConnect.Enabled = false;
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            galaxy = galaxies[galaxyList.SelectedItem.ToString()];
            if (checkBox1.Checked)
            {
                galaxy.Login(textBox1.Text, textBox2.Text);
            }
            else
            {
                galaxy.Login("", "");
            }

            cmdResult = galaxy.CommandResult;
            if (!cmdResult.Successful)
            {
                MessageBox.Show("Failed to login, have you set your authentication correctly?");
                _loggedIn = false;
            }
            else
            {
                _loggedIn = true;

                GetAllObjects();
                objectView.Columns.Add("original", "Original");
                objectView.Columns.Add("renamed", "Re-named");
                objectView.Columns.Add("checkedout", "Checked Out");
                objectView.Columns.Add("template", "Template");

                foreach (var item in myObjects)
                {
                    objectView.Rows.Add(item.objectName, item.objectRename, item.checkedOut, item.template);
                }

                //objectView.DataSource = myObjects;
                //objectView.Refresh();
            }
        }

        private bool GetAllObjects()
        {
            GalaxyObjects = galaxy.QueryObjects(EgObjectIsTemplateOrInstance.gObjectIsTemplate, EConditionType.NameEquals, "thisshoulodnotmatch", EMatch.NotMatchCondition);
            foreach (IgObject myobject in GalaxyObjects)
            {
                bool _checkout = false;
                if (myobject.CheckoutStatus == ECheckoutStatus.notCheckedOut)
                {
                    _checkout = false;
                }
                else
                {
                    _checkout = true;
                }

                myObjects.Add(new MyObject { objectName = myobject.Tagname, objectRename = "", checkedOut = _checkout, template = true });
            }
            GalaxyObjects = galaxy.QueryObjects(EgObjectIsTemplateOrInstance.gObjectIsInstance, EConditionType.NameEquals, "thisshoulodnotmatch", EMatch.NotMatchCondition);
            foreach (IgObject myobject in GalaxyObjects)
            {
                bool _checkout = false;
                if (myobject.CheckoutStatus == ECheckoutStatus.notCheckedOut)
                {
                    _checkout = false;
                }
                else
                {
                    _checkout = true;
                }

                myObjects.Add(new MyObject { objectName = myobject.Tagname, objectRename = "", checkedOut = _checkout, template = false });
            }
            return false;
        }

        class MyObject
        {
            public string objectName;
            public string objectRename;
            public bool checkedOut;
            public bool template;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in objectView.Rows)
            {
                if (row.Cells["checkedout"].Value.ToString() == "False")
                {
                    if (row.Cells["renamed"].Value.ToString() != "")
                    {
                        if (row.Cells["template"].Value.ToString() == "True")
                        {
                            GalaxyObjects = galaxy.QueryObjects(EgObjectIsTemplateOrInstance.gObjectIsTemplate, EConditionType.NameEquals, row.Cells["original"].Value.ToString(), EMatch.MatchCondition);
                            template = (ITemplate)GalaxyObjects[1];
                            template.Tagname = row.Cells["renamed"].Value.ToString();
                        }
                        else
                        {
                            GalaxyObjects = galaxy.QueryObjects(EgObjectIsTemplateOrInstance.gObjectIsInstance, EConditionType.NameEquals, row.Cells["original"].Value.ToString(), EMatch.MatchCondition);
                            instance = (IInstance)GalaxyObjects[1];
                            instance.Tagname = row.Cells["renamed"].Value.ToString();
                        }
                    }
                }
            }
        }
    }

}
