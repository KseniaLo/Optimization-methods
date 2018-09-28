using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Lab_5
{
    public partial class Form1 : Form
    {
        int n, m;
        double[] u;//потенциалы
        double[] v;//потенциалы
        public Form1()
        {
            InitializeComponent();
        }

        //создание таблицы для заполнения условий задачи
        private void button1_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();
            m = Convert.ToInt32(textBox1.Text);
            n = Convert.ToInt32(textBox2.Text);
            dataGridView1.ColumnCount = n + 2;
            dataGridView1.RowCount = m + 2;

            for (int j = 1; j < dataGridView1.ColumnCount - 1; j++)
                dataGridView1.Rows[0].Cells[j].Value = "B" + j.ToString();
            dataGridView1.Rows[0].Cells[dataGridView1.ColumnCount - 1].Value = "Запасы";
            for (int i = 1; i < dataGridView1.RowCount - 1; i++)
                dataGridView1.Rows[i].Cells[0].Value = "A" + i.ToString();
            dataGridView1.Rows[dataGridView1.RowCount - 1].Cells[0].Value = "Потребности";
            button2.Visible = true;
            button2.Enabled = true;
        }

        //решение поставленной задачи
        private void button2_Click(object sender, EventArgs e)
        {
            double[,] Goods;//товары
            double[,] Price;//стоимость
            double[] Holdings;//запасы
            double[] Needs;//потребности
            
            //проверка на невырожденность
            double sumNeeds = 0, sumHoldings = 0;
            for (int i = 1; i < dataGridView1.RowCount - 1; i++)
                sumHoldings += Convert.ToDouble(dataGridView1.Rows[i].Cells[dataGridView1.ColumnCount-1].Value);
            for (int j = 1; j < dataGridView1.ColumnCount - 1; j++)
                sumNeeds += Convert.ToDouble(dataGridView1.Rows[dataGridView1.RowCount - 1].Cells[j].Value);
            if (sumHoldings < sumNeeds)
                m++;
            if (sumNeeds < sumHoldings)
                n++;

            //задание таблиц
            Price = new double[m, n];
            Goods = new double[m, n];
            Holdings = new double[m];
            Needs = new double[n];
            u = new double[m];
            v = new double[n];

            //загрузка данных из таблицы
            FillIn(n, m, Price, Holdings, Needs);
            
            //строим опорный план
            Goods = NorthWestCorner(n, m, Goods, Price, Holdings, Needs);

            //расчитываем потенциалы
            UandV(n, m, Goods, Price);

            //решаем задачу
            Goods = Potentials(n, m, Goods, Price, Holdings, Needs);

            Output(n, m, Goods, Price, Holdings, Needs);

            string output = "Целевая функция: f(x)= "+f(n,m,Goods,Price).ToString();

            richTextBox1.AppendText(output);
        }

        //загрузка данных из таблицы
        public void FillIn(int n, int m, double[,] Price, double[] Holdings, double[] Needs)
        {
            double sumNeeds = 0, sumHoldings = 0;
            for (int i = 1; i < dataGridView1.RowCount - 1; i++)
                sumHoldings += Convert.ToDouble(dataGridView1.Rows[i].Cells[dataGridView1.ColumnCount - 1].Value);
            for (int j = 1; j < dataGridView1.ColumnCount; j++)
                sumNeeds += Convert.ToDouble(dataGridView1.Rows[dataGridView1.RowCount - 1].Cells[j].Value);
            
            //задание таблиц
            for (int i = 1; i < dataGridView1.RowCount - 1; i++)
                for (int j = 1; j < dataGridView1.ColumnCount-1; j++)
                    Price[i - 1, j - 1] = Convert.ToDouble(dataGridView1.Rows[i].Cells[j].Value);
            for (int i = 1; i < dataGridView1.RowCount - 1; i++)
                Holdings[i - 1] = Convert.ToDouble(dataGridView1.Rows[i].Cells[dataGridView1.ColumnCount - 1].Value);
            for (int j = 1; j < dataGridView1.ColumnCount-1; j++)
                Needs[j - 1] = Convert.ToDouble(dataGridView1.Rows[dataGridView1.RowCount - 1].Cells[j].Value);

            if (sumHoldings < sumNeeds)
            {
                for (int j = 0; j < n; j++)
                    Price[m - 1, j] = 0;
                Holdings[m - 1] = sumNeeds - sumHoldings;
            }
            if (sumNeeds < sumHoldings)
            {
                for (int i = 0; i < m; i++)
                    Price[i, n - 1] = 0;
                Needs[n - 1] = sumHoldings - sumNeeds;
            }
        }

        //метод северо-западного угла для поиска опорного плана
        public double[,] NorthWestCorner(int n, int m, double[,] Goods, double[,] Price, double[] Holdings, double[] Needs)
        {
            double[] Holdings1 = new double[Holdings.Length];
            double[] Needs1 = new double[Needs.Length];
            for (int k = 0; k < Holdings.Length; k++)
                Holdings1[k] = Holdings[k];
            for (int k = 0; k < Needs.Length; k++)
                Needs1[k] = Needs[k];

            int i = 0, j = 0;
            Goods[0, 0] = Math.Min(Holdings1[0], Needs1[0]);
            while (i < m && j < n)
            {
                if (Holdings1[i] - Goods[i, j] == 0 && Needs1[j] - Goods[i, j] == 0)//при одновременном закрытии столбца и строки
                {
                    for (int k = j + 1; k < n; k++)
                        Goods[i, k] = -1;
                    for (int k = i + 1; k < m; k++)
                        Goods[k, j] = -1;
                    i++;
                    j++;
                    Goods[i, j] = Math.Min(Holdings1[i], Needs1[j]);
                }
                else
                {
                    if (Holdings1[i] - Goods[i, j] == 0)//если закрылась строка
                    {
                        for (int k = j + 1; k < n; k++)
                            Goods[i, k] = -1;
                        Holdings1[i] -= Holdings1[i];
                        i++;
                        Needs1[j] -= Goods[i - 1, j];
                        Goods[i, j] = Math.Min(Needs1[j], Holdings1[i]);
                    }
                    else//если закрылся столбец
                    {
                        for (int k = i + 1; k < m; k++)
                            Goods[k, j] = -1;
                        Needs1[j] -= Needs1[j];
                        j++;
                        Holdings1[i] -= Goods[i, j - 1];
                        Goods[i, j] = Math.Min(Holdings1[i], Needs1[j]);
                    }
                }
                if (Goods[m - 1, n - 1] != 0)
                    break;
            }
            //проверяем план на невырожденность
            Goods = Degenerate(n, m, Goods, Price);

            return Goods;
        }

        //проверка плана на невырожденность с помощью равенства n+m-1
        public double[,] Degenerate(int n, int m, double[,] Goods, double[,] Price)
        {
            int amount = 0, i = 0, j = 0;//количество загруженных клеток
            for (i = 0; i < m; i++)
                for (j = 0; j < n; j++)
                    if (Goods[i, j] != -1)
                        amount++;
            if (amount != (m + n - 1))//если план вырожденный
            {
                int count = 0;
                i = 0;
                while (amount != (m + n - 1))
                {
                    for (j = 0; j < n; j++)
                    {
                        if (Goods[i, j] != -1)
                            count++;
                    }
                    if (count <= 2)
                    {
                        double minj = 10E16, min = 10E16;
                        for (j = 0; j < n; j++)
                            if (Price[i, j] < min && Goods[i, j] == -1)
                            {
                                minj = j;
                                min = Price[i, j];
                            }
                        Goods[i, (int)minj] = 0;
                    }
                    count = 0;
                    amount = 0;
                    for (int k = 0; k < m; k++)
                        for (j = 0; j < n; j++)
                            if (Goods[k, j] != -1)
                                amount++;
                    i++;
                }
            }
            return Goods;
        }

        //метод потенциалов решения задачи
        public double[,] Potentials(int n, int m, double[,] Goods, double[,] Price, double[] Holdings, double[] Needs)
        {
            bool end = false;//окончание алгоритма
            double[,] delta = new double[m, n];//относительные оценки для клеток                    
            List<Way> NewWay = new List<Way>();

            while (end == false)
            {
                for (int i = 0; i < m; i++)
                    for (int j = 0; j < n; j++)
                        if (Goods[i, j] == -1)//если клетка свободна
                            delta[i, j] = Price[i, j] - (u[i] + v[j]);//считаем для нее относительную оценку
                
                //поиск наименьшей из отрицательных оценок, если такая существует
                int foundi = -1, foundj = -1, k = 0;
                double error = 0;
                while (k < m)//проходим по всем строкам
                {
                    for (int j = 0; j < n; j++)
                    {
                        if (delta[k, j] < 0 && delta[k, j] <= error)//если оценка отрицательна и меньше другой найденной отрицательной оценки
                        {
                            error = delta[k, j];
                            foundi = k;
                            foundj = j;
                        }
                    }
                    k++;
                }
                if (foundi < 0)
                    end = true;
                if (end != true)
                {
                    NewWay.Add(new Way(foundi, foundj));
                    int wayi = foundi, wayj = foundj;

                    //задание цикла
                    do
                    {
                        int countrows = 1, countcolumns = 1;
                        bool horizontal = false, vertical = false;

                        if (NewWay.Count >= 2)
                        {
                            if (NewWay[NewWay.Count - 1].I == wayi && NewWay[NewWay.Count - 2].I == wayi)//если уже выбрано 2 узла в строке
                                countrows = 2;
                            if (NewWay[NewWay.Count - 1].J == wayj && NewWay[NewWay.Count - 2].J == wayj)//если уже выбрано 2 узла в столбце
                                countcolumns = 2;
                        }
                        else
                            countrows = 2;

                        if (countrows == 2)
                        {                                                                  
                            vertical = true;
                            if (NextStepUp(wayi, wayj, Goods) != -1 && (NextStepLeft(NextStepUp(wayi, wayj, Goods), wayj, Goods) != -1 || NextStepRight(NextStepUp(wayi, wayj, Goods), wayj, Goods) != -1) || NextStepUp(wayi, wayj, Goods) == foundi)
                                wayi = NextStepUp(wayi, wayj, Goods);
                            else
                                if (NextStepDown(wayi, wayj, Goods) != -1 && (NextStepLeft(NextStepDown(wayi, wayj, Goods), wayj, Goods) != -1 || NextStepRight(NextStepDown(wayi, wayj, Goods), wayj, Goods) != -1) || NextStepDown(wayi, wayj, Goods) == foundi)
                                    wayi = NextStepDown(wayi, wayj, Goods);
                        }
                        else
                            if (countcolumns == 2)
                            {                                    
                                horizontal = true;
                                if (NextStepRight(wayi, wayj, Goods) != -1 && (NextStepDown(wayi, NextStepRight(wayi, wayj, Goods), Goods) != -1 || NextStepUp(wayi, NextStepRight(wayi, wayj, Goods), Goods) != -1))
                                    wayj = NextStepRight(wayi, wayj, Goods);
                                else
                                    if (NextStepLeft(wayi, wayj, Goods) != -1 && (NextStepDown(wayi, NextStepLeft(wayi, wayj, Goods), Goods) != -1 || NextStepUp(wayi, NextStepLeft(wayi, wayj, Goods), Goods) != -1))
                                        wayj = NextStepLeft(wayi, wayj, Goods);
                            }
                        if (wayi != NewWay[NewWay.Count - 1].I || wayj != NewWay[NewWay.Count - 1].J)
                            NewWay.Add(new Way(wayi, wayj));
                        else
                        {
                            if (horizontal == true)
                            {
                                if (NextStepRight(wayi, wayj + 1, Goods) != -1)
                                    wayj = NextStepRight(wayi, wayj + 1, Goods);
                                else
                                {
                                    if (NextStepLeft(wayi, wayj - 1, Goods) != -1)
                                        wayj = NextStepLeft(wayi, wayj - 1, Goods);
                                    else
                                    {
                                        if (NewWay[NewWay.Count - 2].I > NewWay[NewWay.Count - 1].I)
                                        {
                                            wayi = NextStepUp(wayi, wayj, Goods);
                                            NewWay.RemoveAt(NewWay.Count - 1);
                                        }
                                        else
                                        {
                                            wayi = NextStepDown(wayi, wayj, Goods);
                                            NewWay.RemoveAt(NewWay.Count - 1);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (NextStepDown(wayi + 1, wayj, Goods) != -1)
                                    wayi = NextStepDown(wayi + 1, wayj, Goods);
                                else
                                {
                                    if (NextStepUp(wayi - 1, wayj, Goods) != -1)
                                        wayi = NextStepUp(wayi - 1, wayj, Goods);
                                    else
                                    {
                                        if (NewWay[NewWay.Count - 2].J > NewWay[NewWay.Count - 1].J)
                                        {
                                            wayj = NextStepLeft(wayi, wayj, Goods);
                                            NewWay.RemoveAt(NewWay.Count - 1);
                                        }
                                        else
                                        {
                                            wayj = NextStepRight(wayi, wayj, Goods);
                                            NewWay.RemoveAt(NewWay.Count - 1);
                                        }
                                    }
                                }
                            }
                            NewWay.Add(new Way(wayi, wayj));
                        }
                    }
                    while (foundi != wayi);


                    //поиск минимального в отрицательных узлах
                    double min=10E16;
                    for (int i = 0; i < NewWay.Count; i++)
                        if (Goods[NewWay[i].I, NewWay[i].J] <= min && Goods[NewWay[i].I, NewWay[i].J] != -1 && i % 2 != 0)
                            min = Goods[NewWay[i].I, NewWay[i].J];

                    for (int i = 0; i < NewWay.Count; i++)
                        if (i % 2 == 0)
                            if (Goods[NewWay[i].I, NewWay[i].J] == -1)
                                Goods[NewWay[i].I, NewWay[i].J] = min;
                            else
                                Goods[NewWay[i].I, NewWay[i].J] += min;
                        else
                        {
                            Goods[NewWay[i].I, NewWay[i].J] -= min;
                            if (Goods[NewWay[i].I, NewWay[i].J] == 0)
                                Goods[NewWay[i].I, NewWay[i].J] = -1;
                        }

                    Degenerate(n,m,Goods,Price);

                    //пересчет потенциалов
                    UandV(n, m, Goods, Price);
                }
                NewWay.Clear();
                for (int i = 0; i < m; i++)
                    for (int j = 0; j < n; j++)
                        delta[i, j] = 0;
            }
            return Goods;
        }

        public int NextStepDown(int k, int j, double[,] Goods)
        {
            int foundi = -1;
            for(int i=k+1;i<m;i++)
                if (Goods[i, j] != -1)
                {
                    foundi = i;
                    break;
                }
            return foundi;
        }

        public int NextStepUp(int k, int j, double[,] Goods)
        {
            int foundi = -1;
            for (int i = k - 1; i >= 0; i--)
                if (Goods[i, j] != -1)
                {
                    foundi = i;
                    break;
                }
            return foundi;
        }

        public int NextStepRight(int i, int k, double[,] Goods)
        {
            int foundj = -1;
            for (int j = k + 1; j < n; j++)
                if (Goods[i, j] != -1)
                {
                    foundj = j;
                    break;
                }
            return foundj;
        }

        public int NextStepLeft(int i, int k, double[,] Goods)
        {
            int foundj = -1;
            for (int j = k - 1; j >= 0; j--)
                if (Goods[i, j] != -1)
                {
                    foundj = j;
                    break;
                }
            return foundj;
        }

        //подсчет потенциалов
        public void UandV(int n, int m, double[,] Goods, double[,] Price)
        {
            for (int i = 0; i < m; i++)
                u[i] = 0;
            for (int j = 0; j < n; j++)
                v[j] = 0;

            int foundi = 0;
            List<UV> newUV = new List<UV>();//создаем список индексов для загруженных клеток
            int t = 0, tt = 0;
            while (t <= (m - 1) || tt <= (n - 1))
            {                
                if (t < m && tt < n)
                {
                    for (int k = tt; k < n; k++)
                        if (Goods[t, k] != -1)//если клетка загружена
                            newUV.Add(new UV(t, k, false, false));//добавляем ее координаты в список
                    for (int k = t + 1; k < m; k++)
                        if (Goods[k, tt] != -1)//если клетка загружена
                            newUV.Add(new UV(k, tt, false, false));//добавляем ее координаты в список
                }
                else
                    if (t == m - 1 && tt == n - 2)
                        tt++;
                t++;
                tt++;

            }

            //ищем первую строку, в которой будет >=2 загруженных клеток
            for (int i = 0; i < m; i++)
            {
                int count = 0;
                for (int j = 0; j < n; j++)
                {
                    if (Goods[i, j] != -1)
                        count++;
                }
                if (count >= 2)
                {
                    foundi = i;
                    break;
                }
            }

            //после того, как такая строка найдена
            for (int i = 0; i < m; i++)
                if (i == foundi)
                {
                    u[i] = 0;//ставим в соответствие потенциалу этой строки 0
                    for (int k = 0; k < newUV.Count; k++)
                    {
                        if (newUV[k].I == foundi)
                            newUV[k].IEXIST = true;// и отмечаем, что эта строка проверена                       
                    }
                }

            bool allichecked = true, alljchecked = true;

            do
            {
                for (int k = 0; k < newUV.Count; k++)
                {
                    if (newUV[k].IEXIST == true)//если строка проверена
                    {
                        v[newUV[k].J] = Price[newUV[k].I, newUV[k].J] - u[newUV[k].I];//находим потенциал столбца
                        int j = newUV[k].J;
                        for (int p = 0; p < newUV.Count; p++)
                            if (newUV[p].J == j)
                                newUV[p].JEXIST = true;
                    }
                    if (newUV[k].JEXIST == true)//если проверен столбец
                        {
                            u[newUV[k].I] = Price[newUV[k].I, newUV[k].J] - v[newUV[k].J];//находим потенциал строки
                            int i = newUV[k].I;
                            for (int p = 0; p < newUV.Count; p++)
                                if (newUV[p].I == i)
                                    newUV[p].IEXIST = true;
                        }
                }
                allichecked = true; alljchecked = true;
                for (int k = 0; k < newUV.Count; k++)
                {
                    if (newUV[k].IEXIST == false)
                    {
                        allichecked = false;
                        break;
                    }
                    if (newUV[k].JEXIST == false)
                    {
                        alljchecked = false;
                        break;
                    }
                }
            } 
            while (allichecked == false || alljchecked == false);

            newUV.Clear();
        }

        public void Output(int n, int m, double[,] Goods, double[,] Price, double[] Holdings, double[] Needs)
        {
            dataGridView1.ColumnCount = n + 3;
            dataGridView1.RowCount = m + 3;

            for (int j = 1; j < dataGridView1.ColumnCount - 2; j++)
                dataGridView1.Rows[0].Cells[j].Value = "B" + j.ToString();
            dataGridView1.Rows[0].Cells[dataGridView1.ColumnCount - 2].Value = "Запасы";
            dataGridView1.Rows[0].Cells[dataGridView1.ColumnCount - 1].Value = "u";
            for (int i = 1; i < dataGridView1.RowCount - 2; i++)
                dataGridView1.Rows[i].Cells[0].Value = "A" + i.ToString();
            dataGridView1.Rows[dataGridView1.RowCount - 2].Cells[0].Value = "Потребности";
            dataGridView1.Rows[dataGridView1.RowCount - 1].Cells[0].Value = "v";

            for (int i = 1; i < dataGridView1.RowCount - 2; i++)
                for (int j = 1; j < dataGridView1.ColumnCount - 2; j++)
                    if (Goods[i - 1, j - 1] != -1)
                        dataGridView1.Rows[i].Cells[j].Value = Goods[i - 1, j - 1].ToString() + "[" + Price[i - 1, j - 1].ToString() + "] ";
                    else
                        dataGridView1.Rows[i].Cells[j].Value = "[" + Price[i - 1, j - 1].ToString() + "] ";

            for (int i = 1; i < dataGridView1.RowCount - 2; i++)
                dataGridView1.Rows[i].Cells[dataGridView1.ColumnCount - 2].Value = Holdings[i - 1].ToString();

            for (int i = 1; i < dataGridView1.RowCount - 2; i++)
                dataGridView1.Rows[i].Cells[dataGridView1.ColumnCount - 1].Value = u[i - 1].ToString();

            for (int j = 1; j < dataGridView1.ColumnCount - 2; j++)
                dataGridView1.Rows[dataGridView1.RowCount - 2].Cells[j].Value = Needs[j - 1].ToString();

            for (int j = 1; j < dataGridView1.ColumnCount - 2; j++)
                dataGridView1.Rows[dataGridView1.RowCount - 1].Cells[j].Value = v[j - 1].ToString();
        }

        //целевая функция
        public double f(int n, int m, double[,] Goods, double[,] Price)
        {
            double sum = 0;

            for (int i = 0; i < m; i++)
                for (int j = 0; j < n; j++)
                    if (Goods[i, j] != -1)
                        sum += Goods[i, j] * Price[i, j];

            return sum;
        }
    }

    class UV//для задания матрицы потенциалов по загруженным клеткам
    {
        int i, j;
        bool iexist, jexist;
        public UV(int i, int j, bool iexist, bool jexist)
        {
            this.i = i;
            this.j = j;
            this.iexist = iexist;
            this.jexist = jexist;
        }

        public int I
        {
            get { return i; }
        }
        public int J
        {
            get { return j; }
        }
        public bool IEXIST
        {
            get { return iexist; }
            set { iexist = value; }
        }
        public bool JEXIST
        {
            get { return jexist; }
            set { jexist = value; }
        }
    }
    class Way//для задания означенного цикла
    {
        int i, j;
        public Way(int i, int j)
        {
            this.i = i;
            this.j = j;
        }

        public int I
        {
            get { return i; }
        }
        public int J
        {
            get { return j; }
        }
    }
}
