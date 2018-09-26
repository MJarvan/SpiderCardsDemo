using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SpiderCardsDemo
{
	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class MainWindow:Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		//纸牌枚举
		public enum Cards
		{
			A = 1,
			N2 = 2,
			N3 = 3,
			N4 = 4,
			N5 = 5,
			N6 = 6,
			N7 = 7,
			N8 = 8,
			N9 = 9,
			N10 = 10,
			J = 11,
			Q = 12,
			K = 13,
		};

		//一共8*13张牌
		private List<int[]> cardlist = new List<int[]>();
		private Random r = new Random();

		//待抽区域有5套一套10张
		private int drawcardnum = 5;
		private List<int[]> drawcardlist = new List<int[]>();

		//游戏区域就是8*13-5*100=54张,但要分成十列,所以有4列多一张
		private List<List<int>> playcardlist = new List<List<int>>();



		private void Window_Loaded(object sender,RoutedEventArgs e)
		{
			RefreshCards();
			PutCardsIntoDrawCardsArea();
			PutCardsIntoPlayArea();
		}

		/// <summary>
		/// 洗牌进去抽卡区
		/// </summary>
		private void PutCardsIntoDrawCardsArea()
		{
			for(int i = 0;i < drawcardnum;i++)
			{
				int[] drawcard = new int[PlayArea.ColumnDefinitions.Count];
				int j = 0;

				foreach(int[] card in cardlist)
				{
					drawcard[j] = card[i];
					card[i] = 0;
					j++;

					if(j == 8)
					{
						drawcard[j] = cardlist[i][drawcardnum];
						cardlist[i][drawcardnum] = 0;
						drawcard[j + 1] = cardlist[i][drawcardnum + 1];
						cardlist[i][drawcardnum + 1] = 0;
					}
				}

				drawcardlist.Add(drawcard);
			}

			//生成5叠待抽的牌
			for(int a = 0;a < drawcardlist.Count;a++)
			{
				Border border = new Border();
				border.BorderThickness = new Thickness(1);
				border.BorderBrush = Brushes.Black;
				Rectangle rect = new Rectangle();
				border.Height = 60;
				border.Width =40;
				rect.Fill = Brushes.Red;
				rect.Tag = drawcardlist[a];
				border.Child = rect;
				rect.MouseLeftButtonDown += Rectangle_MouseLeftButtonDown;
				DrawCardsArea.Children.Add(border);
				Grid.SetColumn(border,a + 1);
			}
		}

		/// <summary>
		/// 点击把牌发到playarea
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Rectangle_MouseLeftButtonDown(object sender,MouseButtonEventArgs e)
		{
			Rectangle rect = sender as Rectangle;
			int[] cards = rect.Tag as int[];
			for(int i = 0;i < cards.Length;i++)
			{

			}
		}

		private void PutCardsIntoPlayArea()
		{
			for(int i = 0;i < PlayArea.ColumnDefinitions.Count;i++)
			{
				plusonecount = 0;
				//前四列要六张
				List<int> playcard = new List<int>();
				int num = drawcardnum;
				if(i < 4)
				{
					ForeachPutCards(playcard,num,6);
				}
				else
				{
					ForeachPutCards(playcard,num,5);
				}
			}


			for(int j = 0;j < playcardlist.Count;j++)
			{
				int a = 0;
				foreach(int num in playcardlist[j])
				{
					Border border = new Border();
					border.BorderThickness = new Thickness(1);
					border.BorderBrush = Brushes.Black;
					border.Height = 120;
					border.Width = 80;
					TextBlock textblock = new TextBlock();
					textblock.Text = num.ToString();
					textblock.Tag = num;
					border.Child = textblock;
					border.VerticalAlignment = VerticalAlignment.Top;
					border.Margin = new Thickness(0,a * 30,0,0);
					PlayArea.Children.Add(border);
					Grid.SetColumn(border,j);
					//Grid.SetZIndex(border,j);
					a++;
				}
			}
		}

		private int plusonecount = 0;
		private void ForeachPutCards(List<int> playcard,int num,int count)
		{
			if(cardlist[plusonecount][num] != 0)
			{
				playcard.Add(cardlist[plusonecount][num]);
				cardlist[plusonecount][num] = 0;
			}
			plusonecount++;

			if(plusonecount == 8)
			{
				num++;
				plusonecount = 0;
			}
			if(playcard.Count >= count)
			{
				playcardlist.Add(playcard);
			}
			else
			{
				ForeachPutCards(playcard,num,count);
			}
		}

		/// <summary>
		/// 洗牌
		/// </summary>
		private void RefreshCards()
		{
			for(int i = 0;i < 8;i++)
			{
				int[] card = new int[13];

				for(int j = 0;j < card.Length;j++)
				{
					CheckCard(card,j);
				}

				cardlist.Add(card);
			}
		}

		private void CheckCard(int[] card ,int j)
		{
			int random = r.Next(1,14);
			if(card.Contains(random))
			{
				CheckCard(card,j);
			}
			else
			{
				card[j] = random;
			}
		}
	}
}
