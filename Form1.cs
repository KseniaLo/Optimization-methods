using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Lab_3
{
    public partial class Form1 : Form
    {
        int[] x;//какие х'ы надо подставлять в функцию
        string helpx = null;//х'ы для f~
        double[] b;
        double[,] A;
        double[] cp;
        double[] cj;
        double[] delta;
        double[] ep;
        double[] min;
        int[] xnum;//номера х'ов из колонки хБ
        double[,] E;
        double[,] Aunited;
        double[] coeffs;
        bool end = true, finish = false, minimum = false, maximum = false;
        int n = 0, m = 0, leadingj = 0, leadingi = 0, morecounter = 0, equalcounter = 0;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();
            dataGridView2.Columns.Clear();
            n = Convert.ToInt32(textBox1.Text);
            m = Convert.ToInt32(textBox2.Text);

            dataGridView1.ColumnCount = 2 * n + 2;
            dataGridView1.RowCount = m;
            dataGridView2.ColumnCount = 2 * n;
            dataGridView2.RowCount = 1;

            for (int j = 0; j < dataGridView1.ColumnCount - 2; j++)
            {
                for (int i = 0; i < dataGridView1.RowCount; i++)
                {
                    if (j % 2 != 0)
                    {
                        dataGridView1.Rows[i].Cells[j].Value = "x" + ((j + 1) / 2).ToString();
                        dataGridView2.Rows[0].Cells[j].Value = "x" + ((j + 1) / 2).ToString();
                    }
                    else
                        if (j >= 1)
                            dataGridView1.Rows[i].Cells[j - 1].Value += "+";
                }
            }
            button2.Enabled = true;
            button2.Visible = true;
            x = new int[n];
            for (int i = 0; i < n; i++)
                x[i] = i + 1;
            xnum = new int[m];
            A = new double[m, n];
            b = new double[m];
            min = new double[m];
            coeffs = new double[n];
        }
        private void button2_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
                maximum = true;
            if (radioButton2.Checked)
                minimum = true;
            if (maximum == false && minimum == false)
            {
                MessageBox.Show("Выберите экстремум!");
            }
            else
            {
                FillIn(n, m, A, E, b);
                cp = new double[m + morecounter + n];
                cj = new double[m];
                ep = new double[n + m + 1 + morecounter];
                delta = new double[n + m + morecounter];
                for (int i = 0; i < dataGridView2.ColumnCount; i++)
                    if (i % 2 == 0)
                        coeffs[i / 2] = Convert.ToDouble(dataGridView2.Rows[0].Cells[i].Value);
                CpCj();

                dataGridView1.ColumnCount = n + m + 3 + morecounter;
                dataGridView1.RowCount = m + 3;

                //считаем дельту по схеме1
                delta = delta1(delta, Aunited, cp, cj, n, m);

                //поиск дельта>0
                end = PositiveDelta(delta, end);
                Show(xnum, b, Aunited, min, delta, ep, n, m, finish);

                //если такая дельта найдена

                if (end == false)
                {
                    //поиск ведущего столбца
                    leadingj = LeadingJ(delta);

                    if (LeadingJCheck(Aunited, leadingj) == false)
                    {
                        richTextBox1.AppendText("Задача не имеет решения");
                        button3.Enabled = false;
                        button3.Visible = false;
                    }
                    else
                    {
                        //расчет минимума
                        min = Minimum(Aunited, b, min, leadingj, m);

                        //поиск ведущей строки
                        leadingi = LeadingI(min, m);

                        //считаем эпсилон
                        ep = Epsilon(Aunited, b, leadingi, leadingj, n, m);

                        //вывод таблицы на экран
                        Show(xnum, b, Aunited, min, delta, ep, n, m, finish);

                        button3.Enabled = true;
                        button3.Visible = true;
                    }
                }
                else
                {
                    finish = true;
                    string answer = null;
                    if (morecounter == 0)
                        answer += "Максимум функции: " + f(xnum, b, n, m, coeffs) + "\n";
                    richTextBox1.AppendText(answer);
                    Show(xnum, b, Aunited, min, delta, ep, n, m, finish);
                }
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            button2.Enabled = false;
            button2.Visible = false;
            if (finish == false)
            {

                //замена имени переменной в ХБ
                xnum[leadingi] = leadingj + 1;

                //пересчет значений таблицы
                NewTable(b, Aunited, min, delta, ep, leadingi, leadingj, n, m);
                leadingj = LeadingJ(delta);
                end = PositiveDelta(delta, end);

                if (end == true)
                {
                    finish = true;
                    string answer = null;
                    if (morecounter == 0 && equalcounter == 0)
                        if (maximum == true)
                            answer += "Максимум функции: " + f(xnum, b, n, m, coeffs) + "\n";
                        else
                            answer += "Минимум функции: " + f(xnum, b, n, m, coeffs) + "\n";
                    else
                    {
                        if (ff(xnum, b, n, m, helpx) == 0)
                        {
                            finish = false;

                            for (int i = 0; i < coeffs.Length; i++)
                            {
                                if (maximum == true)
                                    cp[i] = coeffs[i];
                                else
                                {
                                    if (minimum == true)
                                        cp[i] = -1 * coeffs[i];
                                }
                            }
                            for (int i = n; i < n + m + morecounter; i++)
                                cp[i] = 0;
                            for (int j = 0; j < n + m + morecounter; j++)
                                for (int i = 0; i < m; i++)
                                {
                                    if (xnum[i] - 1 == j)
                                        cj[i] = cp[j];
                                }
                            //morecounter = 0;
                            delta = delta1(delta, Aunited, cp, cj, n, m);
                            end = PositiveDelta(delta, end);
                            if (end == true)
                                finish = true;
                            while (finish == false)
                            {
                                leadingj = LeadingJ(delta);
                                if (LeadingJCheck(Aunited, leadingj) == false)
                                {
                                    answer += "Задача не имеет решения";
                                    button3.Enabled = false;
                                    button3.Visible = false;
                                    finish = true;
                                }
                                else
                                {
                                    min = Minimum(Aunited, b, min, leadingj, m);
                                    leadingi = LeadingI(min, m);
                                    ep = Epsilon(Aunited, b, leadingi, leadingj, n, m);
                                    xnum[leadingi] = leadingj + 1;
                                    NewTable(b, Aunited, min, delta, ep, leadingi, leadingj, n, m);
                                    end = PositiveDelta(delta, end);
                                    if (end == true)
                                        finish = true;
                                }

                                if (end == true)
                                {
                                    finish = true;
                                    if (maximum == true)
                                    answer += "Максимум функции: " + f(xnum, b, n, m, coeffs) + "\n";
                                    else
                                        answer += "Минимум функции: " + f(xnum, b, n, m, coeffs) + "\n";
                                }
                            }
                        }
                        else
                            answer += "Задача не имеет решения";
                    }
                    richTextBox1.AppendText(answer);
                    Show(xnum, b, Aunited, min, delta, ep, n, m, finish);
                }
                else
                {
                    leadingj = LeadingJ(delta);
                    if (LeadingJCheck(Aunited, leadingj) == false)
                    {
                        richTextBox1.AppendText("Задача не имеет решения");
                        button3.Enabled = false;
                        button3.Visible = false;
                    }
                    else
                    {
                        min = Minimum(Aunited, b, min, leadingj, m);
                        leadingi = LeadingI(min, m);
                        ep = Epsilon(Aunited, b, leadingi, leadingj, n, m);
                        Show(xnum, b, Aunited, min, delta, ep, n, m, finish);
                    }
                }

            }
            if (end == true)
            {
                button3.Enabled = false;
                button3.Visible = false;
            }
        }
        public void FillIn(int n, int m, double[,] A, double[,] E, double[] b)//Считывание данных
        {
            for (int i = 0; i < dataGridView1.RowCount; i++)// заполнение массива коэффициентов ограничений A
            {
                for (int j = 0; j < n * 2; j++)
                {
                    if (j % 2 == 0)
                        A[i, j / 2] = Convert.ToDouble(dataGridView1.Rows[i].Cells[j].Value);
                }
            }
            for (int i = 0; i < m; i++)// проверка правой части ограничений на >=0
            {
                if (Convert.ToInt32(dataGridView1.Rows[i].Cells[2 * n + 1].Value) < 0)// если проверяемое значение <0
                {
                    for (int j = 0; j < n; j++)// во всей строке меняем знаки
                        A[i, j] *= -1;
                    dataGridView1.Rows[i].Cells[2 * n + 1].Value = -1 * Convert.ToInt32(dataGridView1.Rows[i].Cells[2 * n + 1].Value);
                    if (String.Compare(dataGridView1.Rows[i].Cells[2 * n].Value.ToString(), "<=") == 0)// если стоял <=
                        dataGridView1.Rows[i].Cells[2 * n].Value = ">=";//меняем на >=
                    else
                    {
                        if (String.Compare(dataGridView1.Rows[i].Cells[2 * n].Value.ToString(), ">=") == 0)// если стоял >=
                            dataGridView1.Rows[i].Cells[2 * n].Value = "<=";//меняем на <=
                    }
                }
            }
            for (int i = 0; i < m; i++)//заполняем значения ограничений
                b[i] = Convert.ToDouble(dataGridView1.Rows[i].Cells[2 * n + 1].Value);

            for (int i = 0; i < m; i++)//считаем сколько получилось знаков >= и =
            {
                if (String.Compare(dataGridView1.Rows[i].Cells[2 * n].Value.ToString(), ">=") == 0)
                    morecounter++;
                if (String.Compare(dataGridView1.Rows[i].Cells[2 * n].Value.ToString(), "=") == 0)
                    equalcounter++;
            }

            if (morecounter == 0 && equalcounter == 0)
            {
                E = new double[m, m];
                for (int i = 0; i < m; i++)
                    for (int j = 0; j < m; j++)
                    {
                        if (i == j)
                            E[i, j] = 1;
                        else
                            E[i, j] = 0;
                    }

                //Объединяем матицы, чтобы были столбцы с У1 по Уn+m
                Aunited = new double[m, m + n];
                for (int i = 0; i < m; i++)
                    for (int j = 0; j < m + n; j++)
                    {
                        if (j < n)
                            Aunited[i, j] = A[i, j];
                        else
                            Aunited[i, j] = E[i, j - n];
                    }
                for (int i = 0; i < m; i++)
                    xnum[i] = i + n + 1;
            }
            else
            {
                int j = 0; string help = null;
                E = new double[m, m + morecounter];//добавляем переменные для канонического вида системы 
                for (int i = 0; i < m; i++)
                {
                    if (String.Compare(dataGridView1.Rows[i].Cells[2 * n].Value.ToString(), "<=") == 0)
                    {
                        for (int k = 0; k < m; k++)
                        {
                            if (i == k)
                                E[k, j] = 1;
                            else
                                E[k, j] = 0;
                        }
                        help += j + 1 + n;
                        j++;
                    }
                    if (String.Compare(dataGridView1.Rows[i].Cells[2 * n].Value.ToString(), ">=") == 0)
                    {
                        for (int k = 0; k < m; k++)
                        {
                            if (i == k)
                                E[k, j] = -1;
                            else
                                E[k, j] = 0;
                        }
                        j++;
                    }
                }

                for (int i = 0; i < m; i++)
                {
                    if (String.Compare(dataGridView1.Rows[i].Cells[2 * n].Value.ToString(), ">=") == 0 || String.Compare(dataGridView1.Rows[i].Cells[2 * n].Value.ToString(), "=") == 0)
                    {
                        helpx += (j + 1 + n).ToString();
                        if (help == null)
                            help += (j + 1 + n).ToString();
                        else
                            help = help.Insert(i, (j + 1 + n).ToString());
                        for (int k = 0; k < m; k++)
                        {
                            if (i == k)
                                E[k, j] = 1;
                            else
                                E[k, j] = 0;
                        }
                        j++;
                    }
                }

                //Объединяем матицы, чтобы были столбцы с У1 по Уn+m+morecounter
                Aunited = new double[m, m + n + morecounter];
                for (int i = 0; i < m; i++)
                    for (int k = 0; k < m + n + morecounter; k++)
                    {
                        if (k < n)
                            Aunited[i, k] = A[i, k];
                        else
                            Aunited[i, k] = E[i, k - n];
                    }

                for (int i = 0; i < help.Length; i++)
                    xnum[i] = Convert.ToInt32(help[i].ToString());
            }
        }
        public void CpCj()
        {
            if (morecounter == 0 && equalcounter == 0)
            {
                for (int i = 0; i < n; i++)
                    if (i % 2 == 0)
                        cp[i / 2] = Convert.ToDouble(dataGridView2.Rows[0].Cells[i].Value);
                for (int i = 0; i < m; i++)
                    cj[i] = 0;
            }
            else
            {
                for (int j = 0; j < n + m + morecounter; j++)
                    for (int k = 0; k < helpx.Length; k++)
                        if (Convert.ToInt32(helpx[k].ToString()) - 1 == j)
                            cp[j] = -1;


                for (int j = 0; j < n + m + morecounter; j++)
                    for (int i = 0; i < m; i++)
                    {
                        if (xnum[i] - 1 == j)
                            cj[i] = cp[j];
                    }
            }


        }
        public double[] delta1(double[] _delta, double[,] Aunited, double[] cp, double[] cj, int n, int m)//считается от объединенных A и E
        {
            for (int j = 0; j < _delta.Length - 1; j++)
            {
                double sum = 0;
                for (int i = 0; i < m; i++)
                    sum += Aunited[i, j] * cj[i];
                _delta[j] = cp[j] - sum;
            }
            return _delta;
        }
        public bool PositiveDelta(double[] delta, bool end)
        {
            for (int j = 0; j < delta.Length; j++)
            {
                if (delta[j] > 0)
                {
                    end = false;
                    break;
                }
                else
                    end = true;
            }
            return end;
        }
        public int LeadingJ(double[] _delta)
        {
            int leadingj = 0;
            double maxvalue = 0;
            for (int j = 0; j < _delta.Length; j++)
                if (_delta[j] > 0)
                {
                    if (_delta[j] > maxvalue)
                    {
                        maxvalue = _delta[j];
                        leadingj = j;
                    }
                }
            return leadingj;
        }
        public bool LeadingJCheck(double[,] Aunited, int leadingj)
        {
            bool allneg = true;
            for (int i = 0; i < m; i++)
                if (Aunited[i, leadingj] > 0)
                {
                    allneg = false;
                    break;
                }
            if (allneg == true)
                return false;
            else
                return true;
        }
        public int LeadingI(double[] min, int m)
        {
            int leadingi = 0;
            double tmp = 10E16;
            for (int i = 0; i < m; i++)
            {
                if (min[i] < tmp && min[i] != -1)
                {
                    leadingi = i;
                    tmp = min[i];
                }
            }
            return leadingi;
        }
        public double[] Minimum(double[,] Aunited, double[] b, double[] min, int leadingj, int m)
        {
            for (int i = 0; i < m; i++)
            {
                if (Aunited[i, leadingj] > 0)
                    min[i] = b[i] / Aunited[i, leadingj];
                else
                    min[i] = -1;//чтобы можно было пропускать эти значения при выборе минимума
            }
            return min;
        }
        public double[] Epsilon(double[,] Aunited, double[] b, int leadingi, int leadingj, int n, int m)
        {
            ep[0] = b[leadingi] / Aunited[leadingi, leadingj];
            for (int j = 0; j < m + n + morecounter; j++)
                ep[j + 1] = Aunited[leadingi, j] / Aunited[leadingi, leadingj];
            return ep;
        }
        public void NewTable(double[] b, double[,] Aunited, double[] min, double[] delta, double[] ep, int leadingi, int leadingj, int n, int m)
        {
            //замена ведущей строки на строку-эпсилон
            b[leadingi] = ep[0];
            for (int j = 0; j < n + m + morecounter; j++)
                Aunited[leadingi, j] = ep[j + 1];

            //пересчет значений таблицы
            double[] tmp = new double[m + 1];
            for (int i = 0; i < m; i++)
                tmp[i] = Aunited[i, leadingj];
            tmp[m] = delta[leadingj];
            for (int i = 0; i < m; i++)
            {
                if (i != leadingi)
                    b[i] -= ep[0] * tmp[i];
            }
            for (int j = 0; j < n + m + morecounter; j++)
                for (int i = 0; i < m; i++)
                {
                    if (i != leadingi)
                        Aunited[i, j] -= ep[j + 1] * tmp[i];
                }

            //пересчет значений дельта по схеме2
            for (int j = 0; j < n + m + morecounter; j++)
                delta[j] -= ep[j + 1] * tmp[m];
        }
        public void Show(int[] xnum, double[] b, double[,] Aunited, double[] min, double[] delta, double[] ep, int n, int m, bool finish)
        {
            for (int j = 0; j < n + m + 3 + morecounter; j++)
            {
                if (j == 0)
                    dataGridView1.Rows[0].Cells[j].Value = "ХБ";
                else
                    dataGridView1.Rows[0].Cells[j].Value = "У" + (j - 1).ToString();
                if (j == n + m + 2 + morecounter)
                    dataGridView1.Rows[0].Cells[j].Value = "min";
            }

            for (int i = 1; i <= m; i++)
                dataGridView1.Rows[i].Cells[0].Value = "x" + xnum[i - 1].ToString();
            for (int i = 1; i <= m; i++)
                dataGridView1.Rows[i].Cells[1].Value = Math.Round(b[i - 1],2).ToString();
            for (int i = 1; i <= m; i++)
                for (int j = 0; j < n + m + morecounter; j++)
                    dataGridView1.Rows[i].Cells[j + 2].Value = Math.Round(Aunited[i - 1, j],2).ToString();
            for (int i = 1; i <= m; i++)
            {
                if (finish == false)
                    if (min[i-1] != -1)
                        dataGridView1.Rows[i].Cells[n + m + 2 + morecounter].Value = Math.Round(min[i - 1], 2).ToString();
                    else
                        dataGridView1.Rows[i].Cells[n + m + 2 + morecounter].Value = "";
                else
                    dataGridView1.Rows[i].Cells[n + m + 2 + morecounter].Value = "";
            }
            for (int j = 0; j < n + m + 2 + morecounter; j++)
            {
                if (j == 0)
                    dataGridView1.Rows[dataGridView1.RowCount - 2].Cells[j].Value = "delta";
                else
                {
                    if (j == 1)
                        dataGridView1.Rows[dataGridView1.RowCount - 2].Cells[j].Value = "-";
                    else
                        dataGridView1.Rows[dataGridView1.RowCount - 2].Cells[j].Value = Math.Round(delta[j - 2],2).ToString();
                }
            }
            for (int j = 0; j < n + m + 2 + morecounter; j++)
            {
                if (j == 0)
                    dataGridView1.Rows[dataGridView1.RowCount - 1].Cells[j].Value = "epsilon";
                else
                    if (finish == false)
                        dataGridView1.Rows[dataGridView1.RowCount - 1].Cells[j].Value = Math.Round(ep[j - 1],2).ToString();
                    else
                        dataGridView1.Rows[dataGridView1.RowCount - 1].Cells[j].Value = "";
            }
        }
        public double ff(int[] xnum, double[] b, int n, int m, string helpx)
        {
            double answer = 0;
            for (int i = 0; i < xnum.Length; i++)
                for (int j = 0; j < helpx.Length; j++)
                    if (xnum[i] == Convert.ToInt32(helpx[j].ToString()))
                        answer += b[i];
            return -1 * answer;
        }
        public double f(int[] xnum, double[] b, int n, int m, double[] coeffs)
        {
            double answer = 0;
            double[] answerX = new double[n + m + morecounter];
            for (int i = 0; i < xnum.Length; i++)
                answerX[xnum[i] - 1] = b[i];
            for (int i = 0; i < coeffs.Length; i++)
                answer += answerX[i] * coeffs[i];
            return answer;
        }
    }
}