using System;
using System.Collections.Generic;
using System.Data;
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

		/// <summary>
		/// 卡牌每列可移动的最大张数
		/// </summary>
		private List<int> cardmovelist = new List<int>();

		/// <summary>
		/// 移动前的border
		/// </summary>
		public List<Border> BorderFormList = new List<Border>();

		/// <summary>
		/// 移动前的border的列数
		/// </summary>
		public int columnCountForm;

		/// <summary>
		/// 移动中的项目鼠标指针图片
		/// </summary>
		public Cursor BorderCursor;

		/// <summary>
		/// 游戏区域实时更新
		/// </summary>
		//private DataTable playDataTable = new DataTable("play");

		private void Window_Loaded(object sender,RoutedEventArgs e)
		{
			RefreshCards();
			PutCardsIntoDrawCardsArea();
			PutCardsIntoPlayArea();
			RefreshPlayArea();
			RefreshConnectedList();
		}

		/// <summary>
		/// 刷新每列最大可以变动数量
		/// </summary>
		private void RefreshConnectedList()
		{
			cardmovelist.Clear();
			for(int i = 0;i < playcardlist.Count;i++)
			{
				int totalnum = 0;
				totalnum = CheckConnected(totalnum,playcardlist[i]);
				cardmovelist.Add(totalnum);
			}

			for(int j = 0;j < cardmovelist.Count;j++)
			{
				//凑够A到K,生成一个已完成的到左下角
				if(cardmovelist[j] == 12)
				{
					Grid grid = GetChildObject<Grid>(PlayArea,"paGrid" + j.ToString());
					grid.Children.RemoveRange(grid.Children.Count - 13,cardmovelist[j] + 1);
					playcardlist[j].RemoveRange(playcardlist[j].Count - 13,cardmovelist[j] + 1);
					Border border = new Border();
					border.BorderThickness = new Thickness(1);
					border.BorderBrush = Brushes.Black;
					Rectangle rect = new Rectangle();
					border.Height = 60;
					border.Width = 40;
					rect.Fill = Brushes.Blue;
					border.Child = rect;
					FinishArea.Children.Add(border);
					Grid.SetColumn(border,FinishArea.Children.Count);
				}
			}

			RefreshFinishArea();
		}

		/// <summary>
		/// 判断胜利条件
		/// </summary>
		private void RefreshFinishArea()
		{
			if(FinishArea.Children.Count == FinishArea.ColumnDefinitions.Count)
			{
				MessageBox.Show("你赢了");
			}
		}

		private int CheckConnected(int totalnum,List<int> list)
		{
			if(list.Count - 2 - totalnum >= 0)
			{
				if(list[list.Count - 1 - totalnum] - list[list.Count - 2 - totalnum] == -1)
				{
					totalnum++;
					totalnum = CheckConnected(totalnum,list);
				}
			}
				
			return totalnum;
		}

		/// <summary>
		/// 刻画游戏区域
		/// </summary>
		private void RefreshPlayArea()
		{
			//添加点击事件
			for(int a = 0;a < PlayArea.ColumnDefinitions.Count;a++)
			{
				//playDataTable.Columns.Add(a.ToString(),typeof(int));
				Grid grid = GetChildObject<Grid>(PlayArea,"paGrid" + a.ToString());
				grid.MouseLeftButtonDown += Grid_MouseLeftButtonDown;
			}
			////形成直观datatable
			//for(int i = 0;i < 6;i++)
			//{
			//	DataRow dr = playDataTable.NewRow();
			//	for(int j = 0;j < playcardlist.Count;j++)
			//	{
			//		if(i == 5 && j >= 4)
			//		{
			//			break;
			//		}
			//		else
			//		{
			//			dr[j] = playcardlist[j][i];
			//		}
			//	}
			//	playDataTable.Rows.Add(dr);
			//}
			////
			//for(int i = 0;i < playDataTable.Columns.Count;i++)
			//{
			//	if(i < 4)
			//	{
			//		(int)playDataTable.Rows[6][i] - (int)playDataTable.Rows[6][i]
			//	}
			//}
		}


		private void Grid_MouseLeftButtonDown(object sender,MouseButtonEventArgs e)
		{
			//移动此列
			if(BorderFormList.Count == 0)
			{
				if(e.Source.GetType() == typeof(TextBlock))
				{
					TextBlock textblock = e.Source as TextBlock;
					Border border = textblock.Parent as Border;
					//int borderNum = (int)border.Tag / 30;
					Grid grid = border.Parent as Grid;
					int borderNum = grid.Children.IndexOf(border);
					int columnNum = PlayArea.Children.IndexOf(grid);
					//可以移动一个或者多个
					if(cardmovelist[columnNum] > 0)
					{
						int moveNum = grid.Children.Count - borderNum;
						if(moveNum <= cardmovelist[columnNum] + 1)
						{
							for(int i = 0;i < moveNum;i++)
							{
								this.BorderFormList.Add(grid.Children[grid.Children.Count - 1 - i] as Border);
							}
							this.columnCountForm = columnNum;
							this.BorderCursor = BitmapCursor.CreateBmpCursor(ImageSourceHelper.BitmapSourceMemoryStreamToBitmap(ImageSourceHelper.BytesToBitmapImage(ImageSourceHelper.UIElementToBytes(border,1,1 + 0.25 * (moveNum - 1)))));
							this.PlayArea.Cursor = this.BorderCursor;
						}
					}
					//只有一个可以移动
					else
					{
						this.BorderFormList.Add(border);
						this.columnCountForm = columnNum;
						this.BorderCursor = BitmapCursor.CreateBmpCursor(ImageSourceHelper.BitmapSourceMemoryStreamToBitmap(ImageSourceHelper.BytesToBitmapImage(ImageSourceHelper.UIElementToBytes(border,1,1))));
						this.PlayArea.Cursor = this.BorderCursor;
					}
				}
				//空位
				else if(e.Source.GetType() == typeof(Grid))
				{
					return;
				}
			}
			//放在此列
			else
			{
				for(int i = BorderFormList.Count - 1;i >= 0;i--)
				{
					int num = (int)((TextBlock)this.BorderFormList[i].Child).Tag;
					if(e.Source.GetType() == typeof(TextBlock))
					{
						TextBlock textblock = e.Source as TextBlock;
						Border border = textblock.Parent as Border;
						int borderNum = (int)border.Tag / 30;
						Grid grid = border.Parent as Grid;
						int columnNum = PlayArea.Children.IndexOf(grid);

						//跟本列最后一个比较

						//if(this.columnCountForm == columnNum)
						//{
						//	ClearCursor(true);
						//	return;
						//}

						//可以放
						int lastbordernum = (int)(this.BorderFormList[i].Child as TextBlock).Tag;
						if(playcardlist[columnNum][playcardlist[columnNum].Count - 1] - lastbordernum == 1)
						{
							Border borderTo = CreateCard(num);
							borderTo.Margin = new Thickness(0,grid.Children.Count * 30,0,0);
							borderTo.Tag = grid.Children.Count * 30;
							grid.Children.Add(borderTo);
							playcardlist[columnNum].Add(num);
						}
						//回归原位
						else
						{
							ClearCursor(true);
							return;
						}
					}
					else if(e.Source.GetType() == typeof(Grid))
					{
						//判断grid是否为空 如果是空可以把K放进去
						Grid grid = e.Source as Grid;
						int columnNum = PlayArea.Children.IndexOf(grid);
						int lastbordernum = (int)(this.BorderFormList[i].Child as TextBlock).Tag;
						if(playcardlist[columnNum][playcardlist[columnNum].Count - 1] - lastbordernum == 1)
						{
							Border borderTo = CreateCard(num);
							borderTo.Margin = new Thickness(0,grid.Children.Count * 30,0,0);
							borderTo.Tag = grid.Children.Count * 30;
							grid.Children.Add(borderTo);
							playcardlist[columnNum].Add(num);
							
						}
						else
						{
							ClearCursor(true);
							return;
						}
					}
				}

				ClearCursor(false);
			}
		}

		/// <summary>
		/// 清除所有指标
		/// </summary>
		private void ClearCursor(bool isSameColumn)
		{
			if(!isSameColumn)
			{
				for(int i = 0;i < this.BorderFormList.Count;i++)
				{
					Grid grid = GetChildObject<Grid>(PlayArea,"paGrid" + this.columnCountForm.ToString());
					playcardlist[this.columnCountForm].RemoveAt(playcardlist[this.columnCountForm].Count - 1);
					grid.Children.Remove(this.BorderFormList[i]);
				}
			}
			this.BorderFormList.Clear();
			this.PlayArea.Cursor = Cursors.Arrow;
			RefreshConnectedList();
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
				Border border = CreateCard(cards[i]);
				Grid grid = GetChildObject<Grid>(PlayArea,"paGrid" + i.ToString());
				border.Margin = new Thickness(0,grid.Children.Count * 30,0,0);
				border.Tag = grid.Children.Count * 30;
				grid.Children.Add(border);
			}
			Border rectborder = rect.Parent as Border;

			rectborder.Visibility = Visibility.Collapsed;
		}

		/// <summary>
		/// 放卡到区域
		/// </summary>
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
					Border border = CreateCard(num);
					Grid grid = GetChildObject<Grid>(PlayArea,"paGrid" + j.ToString());
					border.Margin = new Thickness(0,a * 30,0,0);
					border.Tag = a * 30;
					grid.Children.Add(border);
					a++;
				}
			}
		}

		/// <summary>
		/// 生成一张卡牌
		/// </summary>
		/// <param name="num"></param>
		/// <returns></returns>
		private Border CreateCard(int num)
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
			return border;
		}

		private int plusonecount = 0;
		/// <summary>
		/// 循环放count张卡
		/// </summary>
		/// <param name="playcard"></param>
		/// <param name="num"></param>
		/// <param name="count"></param>
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
					RamdonCard(card,j);
				}

				cardlist.Add(card);
			}
		}
		/// <summary>
		/// 随机A-K点数
		/// </summary>
		/// <param name="card"></param>
		/// <param name="j"></param>
		private void RamdonCard(int[] card ,int j)
		{
			int random = r.Next(1,14);
			if(card.Contains(random))
			{
				RamdonCard(card,j);
			}
			else
			{
				card[j] = random;
			}
		}

		/// <summary>
		/// 父控件+控件名找到子控件
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		public T GetChildObject<T>(DependencyObject obj,string name) where T : FrameworkElement
		{
			DependencyObject child = null;
			T grandChild = null;
			for(int i = 0;i <= VisualTreeHelper.GetChildrenCount(obj) - 1;i++)
			{
				child = VisualTreeHelper.GetChild(obj,i);
				if(child is T && (((T)child).Name == name || string.IsNullOrEmpty(name)))
				{
					return (T)child;
				}
				else
				{
					grandChild = GetChildObject<T>(child,name);
					if(grandChild != null)
						return grandChild;
				}
			}
			return null;
		}

	}
}
