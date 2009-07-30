using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ConfusionUtilities;
using System.IO;

namespace KbaseWorkbench
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.Disposed += new EventHandler(Form1_Disposed);
            Logger.Init(true);
        }

        void Form1_Disposed(object sender, EventArgs e)
        {
            if (externalSnippet != null) {
                externalSnippet.shutDown();
            }
            Logger.ShutDown();
        }

        ExternalSnippet externalSnippet;

        private void button1_Click(object sender, EventArgs e)
        {
            externalSnippet = new ExternalSnippet("joe.txt", "Sed et est nibh. Nunc varius odio id lacus molestie quis mattis justo cursus. Vestibulum malesuada volutpat magna ut commodo. Vestibulum purus arcu, fermentum vitae vestibulum a, pulvinar non quam. Vestibulum varius orci ac lacus consectetur ut dapibus purus mattis. Morbi cursus orci nec est mollis eu congue magna gravida. Ut tincidunt elementum porttitor. Nunc augue nisl, molestie sed faucibus eget, iaculis at risus. Sed augue dui, cursus vel posuere sed, fringilla ac velit. Maecenas viverra nisi at augue congue lacinia. Sed varius, sem convallis accumsan blandit, tellus turpis bibendum nunc, at vestibulum neque magna consequat sem. Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Sed convallis dolor in nibh ultrices feugiat. Duis dapibus, enim nec facilisis imperdiet, velit nibh scelerisque purus, sed vulputate turpis nisl a elit. Donec malesuada volutpat libero eget lacinia. Quisque dignissim vulputate sapien sed posuere. Quisque a purus magna, sed aliquet libero. In scelerisque ultrices arcu nec volutpat.");
        }

    }
}
