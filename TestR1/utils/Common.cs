//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Windows.Forms;
//using System.Collections;
//using System.Reflection;

//namespace UHFAPP
//{
//    public class Common
//    {
//        public static bool isEnglish = false;
//        public static string tag = "";
//        public static int bank = 0;


//        public static int time = 2000;

//        private static Hashtable hash = new Hashtable();
       
//        private static Dictionary<string, Hashtable> dictionary = new Dictionary<string, Hashtable>();

//        public static Form GetForm(string FormName, Form mainForm)
//        {
//            if (hash.Contains(FormName))
//            {
//                Form frm = (Form)hash[FormName];
//                return frm;
//            }
//            else
//            {

//                Form form = (Form)Assembly.Load("UHFAPP").CreateInstance("UHFAPP." + FormName); //注意: 窗体命  名空间后面一定要加一个点
//                //form.MdiParent = mainForm; //如果是Mdi窗体的话请加这一行
//                hash.Add(FormName, form);
//                //form.Show();
//                return form;
//            }

//        }

//        public static void SaveForm(Form form)
//        {
//            if (hash.Contains(form.Name))
//            {
//                hash[form.Name] = form;
//            }
//            else
//            {
//                hash.Add(form.Name, form);
//            }
//        }
//        public static Form GetForm(string FormName)
//        {
//            if (hash.Contains(FormName))
//            {
//                return (Form)hash[FormName];
//            }
//            else
//            {
//                return null;
//            }
//        }
//        public static void SaveControlValues(Form FormName)
//        {
//            System.Windows.Forms.Control.ControlCollection sonControls = FormName.Controls;
//            Hashtable hash = new Hashtable();

//            GetControlValues(hash, sonControls);

//            if (dictionary.ContainsKey(FormName.Name))
//            {
//                dictionary[FormName.Name] = hash;
//            }
//            else
//            {
//                dictionary.Add(FormName.Name, hash);
//            }
//        }
//        private static void GetControlValues(Hashtable hash, System.Windows.Forms.Control.ControlCollection sonControls)
//        {
//            foreach (Control control in sonControls)
//            {
//                if (control is TextBox)
//                {
//                    hash.Add(control.Name, control.Text);
//                }
//                else if (control is RadioButton)
//                {
//                    hash.Add(control.Name, ((RadioButton)control).Checked);
//                }
//                else if (control is ComboBox)
//                {
//                    hash.Add(control.Name, ((ComboBox)control).SelectedIndex);
//                }
//                else if (control is CheckBox)
//                {
//                    hash.Add(control.Name, ((CheckBox)control).Checked);
//                }
//                else if (control is Panel)
//                {
//                    GetControlValues(hash, control.Controls);
//                }
//                else if (control is GroupBox)
//                {
//                    GetControlValues(hash, control.Controls);
//                }
//            }
//        }

//        public static Hashtable GetControlValues(string FormName)
//        {
//            if (dictionary.ContainsKey(FormName))
//            {
//                return (Hashtable)dictionary[FormName];
//            }
//            else
//            {
//                return null;
//            }
//        }

//    }

  
//}
