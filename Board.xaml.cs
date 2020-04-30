using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// Die Vorlage "Leere Seite" ist unter http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 dokumentiert.

namespace SolitaireChess
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class Board : Page
    {
        //private string PuzzleRepresentation;
        private string originalPuzzleRepresentation;
        private Image currentImage;
        private Dictionary<Image, KeyValuePair<int, int>> images = new Dictionary<Image, KeyValuePair<int, int>>();
        private List<Move> moves = new List<Move>();
        private Puzzle puzzle = new Puzzle();
        private List<Image> capturedPieces = new List<Image>();
        private bool hintsUsed = false;
        private Rectangle[][] rects = null;
        private bool solchessMode = true;

        public Board()
        {
            this.InitializeComponent();
            rects = new Rectangle[Puzzle.SIZE][];
            for(int n = 0; n<Puzzle.SIZE; n++)
            {
                rects[n] = new Rectangle[Puzzle.SIZE];
            }
            BoardGrid.Width = BoardGrid.Height = 80*(Puzzle.SIZE);
            for(int i=0; i<Puzzle.SIZE; i++)
            {
                RowDefinition grid_row = new RowDefinition();
                grid_row.MinHeight = 80;
                BoardGrid.RowDefinitions.Add(grid_row);
                for (int j=0; j<Puzzle.SIZE; j++)
                {
                    ColumnDefinition grid_col = new ColumnDefinition();
                    grid_col.MinWidth = 80;
                    BoardGrid.ColumnDefinitions.Add(grid_col);
                    Rectangle r = new Rectangle
                    {
                        AllowDrop = true,
                        Height = 85,
                        Width = 85, 
                        Fill = GetFillColor(i, j)
                    };
                    r.Tag = new KeyValuePair<int, int>(i, j);
                    r.Drop += delegate (object sender, DragEventArgs args)
                    {
                        KeyValuePair<int, int> pair = (KeyValuePair<int, int>) r.Tag;
                        if (puzzle.board[pair.Key][pair.Value].piece != ' ')
                        {
                            Move m = new Move();
                            int row = images[currentImage].Key;
                            int col = images[currentImage].Value;
                            m = m.DoMove(puzzle, row, col, pair.Key, pair.Value);
                            if (m != null)
                            {
                                foreach (Image img in images.Keys)
                                {
                                    if (images[img].Key == pair.Key && images[img].Value == pair.Value && img.Visibility == Visibility.Visible)
                                    {
                                        img.Visibility = Visibility.Collapsed;
                                        capturedPieces.Add(img);
                                        Grid.SetRow(currentImage, pair.Key);
                                        Grid.SetColumn(currentImage, pair.Value);
                                        images[currentImage] = new KeyValuePair<int, int>(pair.Key, pair.Value);
                                        break;
                                    }
                                }
                                moves.Add(m);
                                puzzle.pieceCount--;
                            }
                        }
                    };
                    r.DragStarting += delegate (UIElement e, DragStartingEventArgs a)
                    {
                        
                    };
                    BoardGrid.Children.Add(r);

                    rects[i ][j ] = r;
                   
                    Grid.SetRow(r, i);
                    Grid.SetColumn(r, j);
                }
            }
        }

        private Brush GetFillColor(int i, int j)
        {
            if(i%2 == 0)
            {
                if(j%2 == 0)
                {
                    return GetWhiteBackground();
                } 
                else
                {
                    return GetBlackBackground();
                }
            }
            else
            {
                if (j % 2 == 1)
                {
                    return GetWhiteBackground();
                }
                else
                {
                    return GetBlackBackground();
                }
            }
        }

        private Brush GetBlackBackground()
        {
            return new SolidColorBrush(Colors.Black);
        }

        private Brush GetWhiteBackground()
        {
            return new SolidColorBrush(Colors.White);
        }

        private void HidePossibilities()
        {
            for(int i=0; i<Puzzle.SIZE; i++)
            {
                for(int j=0; j<Puzzle.SIZE; j++)
                {
                    rects[i][j].Fill = GetFillColor(i, j);
                }
            }
        }

        private bool IsCheck(int row, int col)
        {
            char p = puzzle.board[row][col].piece;
            if (p == 'K' || p == 'k')
            {
                if (p == 'K')
                {
                    if (puzzle.board[row][col].n != puzzle.offboard)
                    {
                        char o = puzzle.board[row][col].n.piece;
                        if (o == 'q' || o == 'r')
                        {
                            return true;
                        }
                    }
                    if (puzzle.board[row][col].ne != puzzle.offboard)
                    {
                        char o = puzzle.board[row][col].ne.piece;
                        if (o == 'p' || o == 'b' || o == 'q')
                        {
                            return true;
                        }
                    }
                    if (puzzle.board[row][col].e != puzzle.offboard)
                    {
                        char o = puzzle.board[row][col].e.piece;
                        if (o == 'q' || o == 'r')
                        {
                            return true;
                        }
                    }
                    if (puzzle.board[row][col].se != puzzle.offboard)
                    {
                        char o = puzzle.board[row][col].se.piece;
                        if (o == 'b' || o == 'q')
                        {
                            return true;
                        }
                    }
                    if (puzzle.board[row][col].s != puzzle.offboard)
                    {
                        char o = puzzle.board[row][col].s.piece;
                        if (o == 'q' || o == 'r')
                        {
                            return true;
                        }
                    }
                    if (puzzle.board[row][col].sw != puzzle.offboard)
                    {
                        char o = puzzle.board[row][col].sw.piece;
                        if (o == 'b' || o == 'q')
                        {
                            return true;
                        }
                    }
                    if (puzzle.board[row][col].w != puzzle.offboard)
                    {
                        char o = puzzle.board[row][col].w.piece;
                        if (o == 'q' || o == 'r')
                        {
                            return true;
                        }
                    }
                    if (puzzle.board[row][col].nw != puzzle.offboard)
                    {
                        char o = puzzle.board[row][col].nw.piece;
                        if (o == 'p' || o == 'b' || o == 'q')
                        {
                            return true;
                        }
                    }
                    if (puzzle.board[row][col].n != puzzle.offboard)
                    {
                        if (puzzle.board[row][col].n.n != puzzle.offboard)
                        {
                            if (puzzle.board[row][col].n.n.e != puzzle.offboard)
                            {
                                if (puzzle.board[row][col].n.n.e.piece == 'n')
                                {
                                    return true;
                                }
                            }
                            if (puzzle.board[row][col].n.n.w != puzzle.offboard)
                            {
                                if (puzzle.board[row][col].n.n.w.piece == 'n')
                                {
                                    return true;
                                }
                            }
                        }
                        if (puzzle.board[row][col].n.e != puzzle.offboard && puzzle.board[row][col].n.e.e != puzzle.offboard)
                        {
                            if (puzzle.board[row][col].n.e.e.piece == 'n')
                            {
                                return true;
                            }
                        }
                        if (puzzle.board[row][col].n.w != puzzle.offboard && puzzle.board[row][col].n.w.w != puzzle.offboard)
                        {
                            if (puzzle.board[row][col].n.w.w.piece == 'n')
                            {
                                return true;
                            }
                        }

                        if (puzzle.board[row][col].s != puzzle.offboard)
                        {
                            if (puzzle.board[row][col].s.s != puzzle.offboard)
                            {
                                if (puzzle.board[row][col].s.s.e != puzzle.offboard)
                                {
                                    if (puzzle.board[row][col].s.s.e.piece == 'n')
                                    {
                                        return true;
                                    }
                                }
                                if (puzzle.board[row][col].s.s.w != puzzle.offboard)
                                {
                                    if (puzzle.board[row][col].s.s.w.piece == 'n')
                                    {
                                        return true;
                                    }
                                }
                            }
                            if (puzzle.board[row][col].s.e != puzzle.offboard && puzzle.board[row][col].s.e.e != puzzle.offboard)
                            {
                                if (puzzle.board[row][col].s.e.e.piece == 'n')
                                {
                                    return true;
                                }
                            }
                            if (puzzle.board[row][col].s.w != puzzle.offboard && puzzle.board[row][col].s.w.w != puzzle.offboard)
                            {
                                if (puzzle.board[row][col].s.w.w.piece == 'n')
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    Field currentField = puzzle.board[row][col].n;
                    while(currentField != puzzle.offboard && currentField.piece == ' ')
                    {
                        currentField = currentField.n;
                    }
                    if(currentField != puzzle.offboard)
                    {
                        if(currentField.piece == 'q' || currentField.piece == 'r')
                        {
                            return true;
                        }
                    }

                    currentField = puzzle.board[row][col].ne;
                    while (currentField != puzzle.offboard && currentField.piece == ' ')
                    {
                        currentField = currentField.ne;
                    }
                    if (currentField != puzzle.offboard)
                    {
                        if (currentField.piece == 'b' || currentField.piece == 'q')
                        {
                            return true;
                        }
                    }

                    currentField = puzzle.board[row][col].e;
                    while (currentField != puzzle.offboard && currentField.piece == ' ')
                    {
                        currentField = currentField.e;
                    }
                    if (currentField != puzzle.offboard)
                    {
                        if (currentField.piece == 'r' || currentField.piece == 'q')
                        {
                            return true;
                        }
                    }

                    currentField = puzzle.board[row][col].se;
                    while (currentField != puzzle.offboard && currentField.piece == ' ')
                    {
                        currentField = currentField.se;
                    }
                    if (currentField != puzzle.offboard)
                    {
                        if (currentField.piece == 'b' || currentField.piece == 'q')
                        {
                            return true;
                        }
                    }

                    currentField = puzzle.board[row][col].s;
                    while (currentField != puzzle.offboard && currentField.piece == ' ')
                    {
                        currentField = currentField.s;
                    }
                    if (currentField != puzzle.offboard)
                    {
                        if (currentField.piece == 'r' || currentField.piece == 'q')
                        {
                            return true;
                        }
                    }

                    currentField = puzzle.board[row][col].sw;
                    while (currentField != puzzle.offboard && currentField.piece == ' ')
                    {
                        currentField = currentField.sw;
                    }
                    if (currentField != puzzle.offboard)
                    {
                        if (currentField.piece == 'b' || currentField.piece == 'q')
                        {
                            return true;
                        }
                    }

                    currentField = puzzle.board[row][col].w;
                    while (currentField != puzzle.offboard && currentField.piece == ' ')
                    {
                        currentField = currentField.w;
                    }
                    if (currentField != puzzle.offboard)
                    {
                        if (currentField.piece == 'r' || currentField.piece == 'q')
                        {
                            return true;
                        }
                    }

                    currentField = puzzle.board[row][col].nw;
                    while (currentField != puzzle.offboard && currentField.piece == ' ')
                    {
                        currentField = currentField.nw;
                    }
                    if (currentField != puzzle.offboard)
                    {
                        if (currentField.piece == 'b' || currentField.piece == 'q')
                        {
                            return true;
                        }
                    }

                }
                else if (p == 'k')
                {
                    if (puzzle.board[row][col].n != puzzle.offboard)
                    {
                        char o = puzzle.board[row][col].n.piece;
                        if (o == 'Q' || o == 'R')
                        {
                            return true;
                        }
                    }
                    if (puzzle.board[row][col].ne != puzzle.offboard)
                    {
                        char o = puzzle.board[row][col].ne.piece;
                        if (o == 'B' || o == 'Q')
                        {
                            return true;
                        }
                    }
                    if (puzzle.board[row][col].e != puzzle.offboard)
                    {
                        char o = puzzle.board[row][col].e.piece;
                        if (o == 'Q' || o == 'R')
                        {
                            return true;
                        }
                    }
                    if (puzzle.board[row][col].se != puzzle.offboard)
                    {
                        char o = puzzle.board[row][col].se.piece;
                        if (o == 'P' || o == 'B' || o == 'Q')
                        {
                            return true;
                        }
                    }
                    if (puzzle.board[row][col].s != puzzle.offboard)
                    {
                        char o = puzzle.board[row][col].s.piece;
                        if (o == 'Q' || o == 'R')
                        {
                            return true;
                        }
                    }
                    if (puzzle.board[row][col].sw != puzzle.offboard)
                    {
                        char o = puzzle.board[row][col].sw.piece;
                        if (o == 'P' || o == 'B' || o == 'Q')
                        {
                            return true;
                        }
                    }
                    if (puzzle.board[row][col].w != puzzle.offboard)
                    {
                        char o = puzzle.board[row][col].w.piece;
                        if (o == 'Q' || o == 'R')
                        {
                            return true;
                        }
                    }
                    if (puzzle.board[row][col].nw != puzzle.offboard)
                    {
                        char o = puzzle.board[row][col].nw.piece;
                        if (o == 'B' || o == 'Q')
                        {
                            return true;
                        }
                    }
                    if (puzzle.board[row][col].n != puzzle.offboard)
                    {
                        if (puzzle.board[row][col].n.n != puzzle.offboard)
                        {
                            if (puzzle.board[row][col].n.n.e != puzzle.offboard)
                            {
                                if (puzzle.board[row][col].n.n.e.piece == 'N')
                                {
                                    return true;
                                }
                            }
                            if (puzzle.board[row][col].n.n.w != puzzle.offboard)
                            {
                                if (puzzle.board[row][col].n.n.w.piece == 'N')
                                {
                                    return true;
                                }
                            }
                        }
                        if (puzzle.board[row][col].n.e != puzzle.offboard && puzzle.board[row][col].n.e.e != puzzle.offboard)
                        {
                            if (puzzle.board[row][col].n.e.e.piece == 'N')
                            {
                                return true;
                            }
                        }
                        if (puzzle.board[row][col].n.w != puzzle.offboard && puzzle.board[row][col].n.w.w != puzzle.offboard)
                        {
                            if (puzzle.board[row][col].n.w.w.piece == 'N')
                            {
                                return true;
                            }
                        }
                    }

                    if (puzzle.board[row][col].s != puzzle.offboard)
                    {
                        if (puzzle.board[row][col].s.s != puzzle.offboard)
                        {
                            if (puzzle.board[row][col].s.s.e != puzzle.offboard)
                            {
                                if (puzzle.board[row][col].s.s.e.piece == 'N')
                                {
                                    return true;
                                }
                            }
                            if (puzzle.board[row][col].s.s.w != puzzle.offboard)
                            {
                                if (puzzle.board[row][col].s.s.w.piece == 'N')
                                {
                                    return true;
                                }
                            }
                        }
                        if (puzzle.board[row][col].s.e != puzzle.offboard && puzzle.board[row][col].s.e.e != puzzle.offboard)
                        {
                            if (puzzle.board[row][col].s.e.e.piece == 'N')
                            {
                                return true;
                            }
                        }
                        if (puzzle.board[row][col].s.w != puzzle.offboard && puzzle.board[row][col].s.w.w != puzzle.offboard)
                        {
                            if (puzzle.board[row][col].s.w.w.piece == 'N')
                            {
                                return true;
                            }
                        }
                    }


                    Field currentField = puzzle.board[row][col].n;
                    while (currentField != puzzle.offboard && currentField.piece == ' ')
                    {
                        currentField = currentField.n;
                    }
                    if (currentField != puzzle.offboard)
                    {
                        if (currentField.piece == 'Q' || currentField.piece == 'R')
                        {
                            return true;
                        }
                    }

                    currentField = puzzle.board[row][col].ne;
                    while (currentField != puzzle.offboard && currentField.piece == ' ')
                    {
                        currentField = currentField.ne;
                    }
                    if (currentField != puzzle.offboard)
                    {
                        if (currentField.piece == 'B' || currentField.piece == 'Q')
                        {
                            return true;
                        }
                    }

                    currentField = puzzle.board[row][col].e;
                    while (currentField != puzzle.offboard && currentField.piece == ' ')
                    {
                        currentField = currentField.e;
                    }
                    if (currentField != puzzle.offboard)
                    {
                        if (currentField.piece == 'R' || currentField.piece == 'Q')
                        {
                            return true;
                        }
                    }

                    currentField = puzzle.board[row][col].se;
                    while (currentField != puzzle.offboard && currentField.piece == ' ')
                    {
                        currentField = currentField.se;
                    }
                    if (currentField != puzzle.offboard)
                    {
                        if (currentField.piece == 'B' || currentField.piece == 'Q')
                        {
                            return true;
                        }
                    }

                    currentField = puzzle.board[row][col].s;
                    while (currentField != puzzle.offboard && currentField.piece == ' ')
                    {
                        currentField = currentField.s;
                    }
                    if (currentField != puzzle.offboard)
                    {
                        if (currentField.piece == 'R' || currentField.piece == 'Q')
                        {
                            return true;
                        }
                    }

                    currentField = puzzle.board[row][col].sw;
                    while (currentField != puzzle.offboard && currentField.piece == ' ')
                    {
                        currentField = currentField.sw;
                    }
                    if (currentField != puzzle.offboard)
                    {
                        if (currentField.piece == 'B' || currentField.piece == 'Q')
                        {
                            return true;
                        }
                    }

                    currentField = puzzle.board[row][col].w;
                    while (currentField != puzzle.offboard && currentField.piece == ' ')
                    {
                        currentField = currentField.w;
                    }
                    if (currentField != puzzle.offboard)
                    {
                        if (currentField.piece == 'R' || currentField.piece == 'Q')
                        {
                            return true;
                        }
                    }

                    currentField = puzzle.board[row][col].nw;
                    while (currentField != puzzle.offboard && currentField.piece == ' ')
                    {
                        currentField = currentField.nw;
                    }
                    if (currentField != puzzle.offboard)
                    {
                        if (currentField.piece == 'B' || currentField.piece == 'Q')
                        {
                            return true;
                        }
                    }

                }
            }
            
            return false;
        }

        private void ShowPossibilities(int row, int col, char v)
        {
            List<KeyValuePair<int, int>> fields = new List<KeyValuePair<int, int>>();
            switch (v)
            {
                case 'p':
                    if(row < Puzzle.SIZE)
                    {
                        if (puzzle.board[row][col].s.piece == ' ')
                        {
                            KeyValuePair<int, int> f = new KeyValuePair<int, int>(puzzle.board[row][col].s.r, puzzle.board[row][col].s.c);
                            fields.Add(f);
                        }
                        if (row == 1)
                        {   
                            if(puzzle.board[row][col].s.s.piece == ' ')
                            {
                                KeyValuePair<int, int> f = new KeyValuePair<int, int>(puzzle.board[row][col].s.s.r, puzzle.board[row][col].s.s.c);
                                fields.Add(f);
                            }
                        }
                        if(puzzle.board[row][col].se != puzzle.offboard && puzzle.board[row][col].se.piece != ' ')
                        {
                            char piece = puzzle.board[row][col].se.piece;
                            if(piece == 'Q' || piece == 'R' || piece == 'P' || piece == 'N' || piece == 'B')
                            {
                                KeyValuePair<int, int> f = new KeyValuePair<int, int>(puzzle.board[row][col].se.r, puzzle.board[row][col].se.c);
                                fields.Add(f);
                            }
                        }
                        if (puzzle.board[row][col].sw != puzzle.offboard && puzzle.board[row][col].sw.piece != ' ')
                        {
                            char piece = puzzle.board[row][col].sw.piece;
                            if (piece == 'Q' || piece == 'R' || piece == 'P' || piece == 'N' || piece == 'B')
                            {
                                KeyValuePair<int, int> f = new KeyValuePair<int, int>(puzzle.board[row][col].sw.r, puzzle.board[row][col].sw.c);
                                fields.Add(f);
                            }
                        }
                    }
                    break;
                case 'P':
                    if(row > 0)
                    {
                        if (puzzle.board[row][col].n.piece == ' ')
                        {
                            KeyValuePair<int, int> f = new KeyValuePair<int, int>(puzzle.board[row][col].n.r, puzzle.board[row][col].n.c);
                            fields.Add(f);
                        }
                        if (row == Puzzle.SIZE - 2)
                        {
                            if (puzzle.board[row][col].n.n.piece == ' ')
                            {
                                KeyValuePair<int, int> f = new KeyValuePair<int, int>(puzzle.board[row][col].n.n.r, puzzle.board[row][col].n.n.c);
                                fields.Add(f);
                            }
                        }
                        if (puzzle.board[row][col].ne != puzzle.offboard && puzzle.board[row][col].ne.piece != ' ')
                        {
                            char piece = puzzle.board[row][col].ne.piece;
                            if ((solchessMode && (piece == 'Q' || piece == 'R' || piece == 'P' || piece == 'N' || piece == 'B')) ||
                                piece == 'q' || piece == 'r' || piece == 'p' || piece == 'n' || piece == 'b')
                            {
                                KeyValuePair<int, int> f = new KeyValuePair<int, int>(puzzle.board[row][col].ne.r, puzzle.board[row][col].ne.c);
                                fields.Add(f);
                            }
                        }
                        if (puzzle.board[row][col].nw != puzzle.offboard && puzzle.board[row][col].nw.piece != ' ')
                        {
                            char piece = puzzle.board[row][col].nw.piece;
                            if ((solchessMode && (piece == 'Q' || piece == 'R' || piece == 'P' || piece == 'N' || piece == 'B')) ||
                                piece == 'q' || piece == 'r' || piece == 'p' || piece == 'n' || piece == 'b')
                            {
                                KeyValuePair<int, int> f = new KeyValuePair<int, int>(puzzle.board[row][col].nw.r, puzzle.board[row][col].nw.c);
                                fields.Add(f);
                            }
                        }

                    }
                    break;
                case 'Q':
                case 'q':
                    Field currentField = puzzle.board[row][col].n;
                    while(currentField != puzzle.offboard && currentField.piece == ' ')
                    {
                        KeyValuePair<int, int> p = new KeyValuePair<int, int>(currentField.r, currentField.c);
                        fields.Add(p);
                        currentField = currentField.n;
                    }
                    if(currentField != puzzle.offboard)
                    {
                        if((v == 'Q' && currentField.piece == 'k' || currentField.piece == 'q' || currentField.piece == 'r' ||
                            currentField.piece == 'b' || currentField.piece == 'n' || currentField.piece == 'p') || (v == 'Q' && solchessMode))
                        {
                            KeyValuePair<int, int> p = new KeyValuePair<int, int>(currentField.r, currentField.c);
                            fields.Add(p);
                        }
                        else if (v == 'q' && currentField.piece == 'K' || currentField.piece == 'Q' || currentField.piece == 'R' ||
                            currentField.piece == 'B' || currentField.piece == 'N' || currentField.piece == 'P')
                        {
                            KeyValuePair<int, int> p = new KeyValuePair<int, int>(currentField.r, currentField.c);
                            fields.Add(p);
                        }

                    }

                    currentField = puzzle.board[row][col].ne;
                    while (currentField != puzzle.offboard && currentField.piece == ' ')
                    {
                        KeyValuePair<int, int> p = new KeyValuePair<int, int>(currentField.r, currentField.c);
                        fields.Add(p);
                        currentField = currentField.ne;
                    }
                    if (currentField != puzzle.offboard)
                    {
                        if ((v == 'Q' && currentField.piece == 'k' || currentField.piece == 'q' || currentField.piece == 'r' ||
                            currentField.piece == 'b' || currentField.piece == 'n' || currentField.piece == 'p') || (v == 'Q' && solchessMode))
                        {
                            KeyValuePair<int, int> p = new KeyValuePair<int, int>(currentField.r, currentField.c);
                            fields.Add(p);
                        }
                        else if (v == 'q' && currentField.piece == 'K' || currentField.piece == 'Q' || currentField.piece == 'R' ||
                            currentField.piece == 'B' || currentField.piece == 'N' || currentField.piece == 'P')
                        {
                            KeyValuePair<int, int> p = new KeyValuePair<int, int>(currentField.r, currentField.c);
                            fields.Add(p);
                        }

                    }
                    currentField = puzzle.board[row][col].e;
                    while (currentField != puzzle.offboard && currentField.piece == ' ')
                    {
                        KeyValuePair<int, int> p = new KeyValuePair<int, int>(currentField.r, currentField.c);
                        fields.Add(p);
                        currentField = currentField.e;
                    }
                    if (currentField != puzzle.offboard)
                    {
                        if ((v == 'Q' && currentField.piece == 'k' || currentField.piece == 'q' || currentField.piece == 'r' ||
                            currentField.piece == 'b' || currentField.piece == 'n' || currentField.piece == 'p') || (v == 'Q' && solchessMode))
                        {
                            KeyValuePair<int, int> p = new KeyValuePair<int, int>(currentField.r, currentField.c);
                            fields.Add(p);
                        }
                        else if (v == 'q' && currentField.piece == 'K' || currentField.piece == 'Q' || currentField.piece == 'R' ||
                            currentField.piece == 'B' || currentField.piece == 'N' || currentField.piece == 'P')
                        {
                            KeyValuePair<int, int> p = new KeyValuePair<int, int>(currentField.r, currentField.c);
                            fields.Add(p);
                        }

                    }
                    currentField = puzzle.board[row][col].se;
                    while (currentField != puzzle.offboard && currentField.piece == ' ')
                    {
                        KeyValuePair<int, int> p = new KeyValuePair<int, int>(currentField.r, currentField.c);
                        fields.Add(p);
                        currentField = currentField.se;
                    }
                    if (currentField != puzzle.offboard)
                    {
                        if ((v == 'Q' && currentField.piece == 'k' || currentField.piece == 'q' || currentField.piece == 'r' ||
                            currentField.piece == 'b' || currentField.piece == 'n' || currentField.piece == 'p') || (v == 'Q' && solchessMode))
                        {
                            KeyValuePair<int, int> p = new KeyValuePair<int, int>(currentField.r, currentField.c);
                            fields.Add(p);
                        }
                        else if (v == 'q' && currentField.piece == 'K' || currentField.piece == 'Q' || currentField.piece == 'R' ||
                            currentField.piece == 'B' || currentField.piece == 'N' || currentField.piece == 'P')
                        {
                            KeyValuePair<int, int> p = new KeyValuePair<int, int>(currentField.r, currentField.c);
                            fields.Add(p);
                        }

                    }
                    currentField = puzzle.board[row][col].s;
                    while (currentField != puzzle.offboard && currentField.piece == ' ')
                    {
                        KeyValuePair<int, int> p = new KeyValuePair<int, int>(currentField.r, currentField.c);
                        fields.Add(p);
                        currentField = currentField.s;
                    }
                    if (currentField != puzzle.offboard)
                    {
                        if ((v == 'Q' && currentField.piece == 'k' || currentField.piece == 'q' || currentField.piece == 'r' ||
                            currentField.piece == 'b' || currentField.piece == 'n' || currentField.piece == 'p') || (v == 'Q' && solchessMode))
                        {
                            KeyValuePair<int, int> p = new KeyValuePair<int, int>(currentField.r, currentField.c);
                            fields.Add(p);
                        }
                        else if (v == 'q' && currentField.piece == 'K' || currentField.piece == 'Q' || currentField.piece == 'R' ||
                            currentField.piece == 'B' || currentField.piece == 'N' || currentField.piece == 'P')
                        {
                            KeyValuePair<int, int> p = new KeyValuePair<int, int>(currentField.r, currentField.c);
                            fields.Add(p);
                        }

                    }
                    currentField = puzzle.board[row][col].sw;
                    while (currentField != puzzle.offboard && currentField.piece == ' ')
                    {
                        KeyValuePair<int, int> p = new KeyValuePair<int, int>(currentField.r, currentField.c);
                        fields.Add(p);
                        currentField = currentField.sw;
                    }
                    if (currentField != puzzle.offboard)
                    {
                        if ((v == 'Q' && currentField.piece == 'k' || currentField.piece == 'q' || currentField.piece == 'r' ||
                            currentField.piece == 'b' || currentField.piece == 'n' || currentField.piece == 'p') || (v == 'Q' && solchessMode))
                        {
                            KeyValuePair<int, int> p = new KeyValuePair<int, int>(currentField.r, currentField.c);
                            fields.Add(p);
                        }
                        else if (v == 'q' && currentField.piece == 'K' || currentField.piece == 'Q' || currentField.piece == 'R' ||
                            currentField.piece == 'B' || currentField.piece == 'N' || currentField.piece == 'P')
                        {
                            KeyValuePair<int, int> p = new KeyValuePair<int, int>(currentField.r, currentField.c);
                            fields.Add(p);
                        }

                    }
                    currentField = puzzle.board[row][col].w;
                    while (currentField != puzzle.offboard && currentField.piece == ' ')
                    {
                        KeyValuePair<int, int> p = new KeyValuePair<int, int>(currentField.r, currentField.c);
                        fields.Add(p);
                        currentField = currentField.w;
                    }
                    if (currentField != puzzle.offboard)
                    {
                        if ((v == 'Q' && currentField.piece == 'k' || currentField.piece == 'q' || currentField.piece == 'r' ||
                            currentField.piece == 'b' || currentField.piece == 'n' || currentField.piece == 'p') || (v == 'Q' && solchessMode))
                        {
                            KeyValuePair<int, int> p = new KeyValuePair<int, int>(currentField.r, currentField.c);
                            fields.Add(p);
                        }
                        else if (v == 'q' && currentField.piece == 'K' || currentField.piece == 'Q' || currentField.piece == 'R' ||
                            currentField.piece == 'B' || currentField.piece == 'N' || currentField.piece == 'P')
                        {
                            KeyValuePair<int, int> p = new KeyValuePair<int, int>(currentField.r, currentField.c);
                            fields.Add(p);
                        }

                    }
                    currentField = puzzle.board[row][col].nw;
                    while (currentField != puzzle.offboard && currentField.piece == ' ')
                    {
                        KeyValuePair<int, int> p = new KeyValuePair<int, int>(currentField.r, currentField.c);
                        fields.Add(p);
                        currentField = currentField.nw;
                    }
                    if (currentField != puzzle.offboard)
                    {
                        if ((v == 'Q' && currentField.piece == 'k' || currentField.piece == 'q' || currentField.piece == 'r' ||
                            currentField.piece == 'b' || currentField.piece == 'n' || currentField.piece == 'p') || (v == 'Q' && solchessMode))
                        {
                            KeyValuePair<int, int> p = new KeyValuePair<int, int>(currentField.r, currentField.c);
                            fields.Add(p);
                        }
                        else if (v == 'q' && currentField.piece == 'K' || currentField.piece == 'Q' || currentField.piece == 'R' ||
                            currentField.piece == 'B' || currentField.piece == 'N' || currentField.piece == 'P')
                        {
                            KeyValuePair<int, int> p = new KeyValuePair<int, int>(currentField.r, currentField.c);
                            fields.Add(p);
                        }

                    }
                    break;
                case 'K':
                case 'k':
                    if(puzzle.board[row][col].n != puzzle.offboard)
                    {
                        if(solchessMode)
                        {
                            KeyValuePair<int, int> f = new KeyValuePair<int, int>(puzzle.board[row][col].n.r, puzzle.board[row][col].n.c);
                            fields.Add(f);
                        }
                        else if(v == 'K')
                        {
                            char p = puzzle.board[row][col].n.piece;
                            if(p == ' ' || p == 'q' || p == 'r' || p == 'b' || p == 'n' || p == 'p')
                            {
                                KeyValuePair<int, int> f = new KeyValuePair<int, int>(puzzle.board[row][col].n.r, puzzle.board[row][col].n.c);
                                fields.Add(f);
                            }
                        }
                        else if(v == 'k')
                        {
                            char p = puzzle.board[row][col].n.piece;
                            if (p == ' ' || p == 'Q' || p == 'R' || p == 'B' || p == 'N' || p == 'P')
                            {
                                KeyValuePair<int, int> f = new KeyValuePair<int, int>(puzzle.board[row][col].n.r, puzzle.board[row][col].n.c);
                                fields.Add(f);
                            }
                        }
                    }
                    if (puzzle.board[row][col].ne != puzzle.offboard)
                    {
                        if (solchessMode)
                        {
                            KeyValuePair<int, int> f = new KeyValuePair<int, int>(puzzle.board[row][col].ne.r, puzzle.board[row][col].ne.c);
                            fields.Add(f);
                        }
                        else if (v == 'K')
                        {
                            char p = puzzle.board[row][col].ne.piece;
                            if (p == ' ' || p == 'q' || p == 'r' || p == 'b' || p == 'n' || p == 'p')
                            {
                                KeyValuePair<int, int> f = new KeyValuePair<int, int>(puzzle.board[row][col].ne.r, puzzle.board[row][col].ne.c);
                                fields.Add(f);
                            }
                        }
                        else if (v == 'k')
                        {
                            char p = puzzle.board[row][col].ne.piece;
                            if (p == ' ' || p == 'Q' || p == 'R' || p == 'B' || p == 'N' || p == 'P')
                            {
                                KeyValuePair<int, int> f = new KeyValuePair<int, int>(puzzle.board[row][col].ne.r, puzzle.board[row][col].ne.c);
                                fields.Add(f);
                            }
                        }
                    }
                    if (puzzle.board[row][col].e != puzzle.offboard)
                    {
                        if (solchessMode)
                        {
                            KeyValuePair<int, int> f = new KeyValuePair<int, int>(puzzle.board[row][col].e.r, puzzle.board[row][col].e.c);
                            fields.Add(f);
                        }
                        else if (v == 'K')
                        {
                            char p = puzzle.board[row][col].e.piece;
                            if (p == ' ' || p == 'q' || p == 'r' || p == 'b' || p == 'n' || p == 'p')
                            {
                                KeyValuePair<int, int> f = new KeyValuePair<int, int>(puzzle.board[row][col].e.r, puzzle.board[row][col].e.c);
                                fields.Add(f);
                            }
                        }
                        else if (v == 'k')
                        {
                            char p = puzzle.board[row][col].e.piece;
                            if (p == ' ' || p == 'Q' || p == 'R' || p == 'B' || p == 'N' || p == 'P')
                            {
                                KeyValuePair<int, int> f = new KeyValuePair<int, int>(puzzle.board[row][col].e.r, puzzle.board[row][col].e.c);
                                fields.Add(f);
                            }
                        }
                    }
                    if (puzzle.board[row][col].se != puzzle.offboard)
                    {
                        if (solchessMode)
                        {
                            KeyValuePair<int, int> f = new KeyValuePair<int, int>(puzzle.board[row][col].se.r, puzzle.board[row][col].se.c);
                            fields.Add(f);
                        }
                        else if (v == 'K')
                        {
                            char p = puzzle.board[row][col].se.piece;
                            if (p == ' ' || p == 'q' || p == 'r' || p == 'b' || p == 'n' || p == 'p')
                            {
                                KeyValuePair<int, int> f = new KeyValuePair<int, int>(puzzle.board[row][col].se.r, puzzle.board[row][col].se.c);
                                fields.Add(f);
                            }
                        }
                        else if (v == 'k')
                        {
                            char p = puzzle.board[row][col].se.piece;
                            if (p == ' ' || p == 'Q' || p == 'R' || p == 'B' || p == 'N' || p == 'P')
                            {
                                KeyValuePair<int, int> f = new KeyValuePair<int, int>(puzzle.board[row][col].se.r, puzzle.board[row][col].se.c);
                                fields.Add(f);
                            }
                        }
                    }
                    if (puzzle.board[row][col].s != puzzle.offboard)
                    {
                        if (solchessMode)
                        {
                            KeyValuePair<int, int> f = new KeyValuePair<int, int>(puzzle.board[row][col].s.r, puzzle.board[row][col].s.c);
                            fields.Add(f);
                        }
                        else if (v == 'K')
                        {
                            char p = puzzle.board[row][col].s.piece;
                            if (p == ' ' || p == 'q' || p == 'r' || p == 'b' || p == 'n' || p == 'p')
                            {
                                KeyValuePair<int, int> f = new KeyValuePair<int, int>(puzzle.board[row][col].s.r, puzzle.board[row][col].s.c);
                                fields.Add(f);
                            }
                        }
                        else if (v == 'k')
                        {
                            char p = puzzle.board[row][col].s.piece;
                            if (p == ' ' || p == 'Q' || p == 'R' || p == 'B' || p == 'N' || p == 'P')
                            {
                                KeyValuePair<int, int> f = new KeyValuePair<int, int>(puzzle.board[row][col].s.r, puzzle.board[row][col].s.c);
                                fields.Add(f);
                            }
                        }
                    }
                    if (puzzle.board[row][col].sw != puzzle.offboard)
                    {
                        if (solchessMode)
                        {
                            KeyValuePair<int, int> f = new KeyValuePair<int, int>(puzzle.board[row][col].sw.r, puzzle.board[row][col].sw.c);
                            fields.Add(f);
                        }
                        else if (v == 'K')
                        {
                            char p = puzzle.board[row][col].sw.piece;
                            if (p == ' ' || p == 'q' || p == 'r' || p == 'b' || p == 'n' || p == 'p')
                            {
                                KeyValuePair<int, int> f = new KeyValuePair<int, int>(puzzle.board[row][col].sw.r, puzzle.board[row][col].sw.c);
                                fields.Add(f);
                            }
                        }
                        else if (v == 'k')
                        {
                            char p = puzzle.board[row][col].sw.piece;
                            if (p == ' ' || p == 'Q' || p == 'R' || p == 'B' || p == 'N' || p == 'P')
                            {
                                KeyValuePair<int, int> f = new KeyValuePair<int, int>(puzzle.board[row][col].sw.r, puzzle.board[row][col].sw.c);
                                fields.Add(f);
                            }
                        }
                    }
                    if (puzzle.board[row][col].w != puzzle.offboard)
                    {
                        if (solchessMode)
                        {
                            KeyValuePair<int, int> f = new KeyValuePair<int, int>(puzzle.board[row][col].w.r, puzzle.board[row][col].w.c);
                            fields.Add(f);
                        }
                        else if (v == 'K')
                        {
                            char p = puzzle.board[row][col].w.piece;
                            if (p == ' ' || p == 'q' || p == 'r' || p == 'b' || p == 'n' || p == 'p')
                            {
                                KeyValuePair<int, int> f = new KeyValuePair<int, int>(puzzle.board[row][col].w.r, puzzle.board[row][col].w.c);
                                fields.Add(f);
                            }
                        }
                        else if (v == 'k')
                        {
                            char p = puzzle.board[row][col].w.piece;
                            if (p == ' ' || p == 'Q' || p == 'R' || p == 'B' || p == 'N' || p == 'P')
                            {
                                KeyValuePair<int, int> f = new KeyValuePair<int, int>(puzzle.board[row][col].w.r, puzzle.board[row][col].w.c);
                                fields.Add(f);
                            }
                        }
                    }
                    if (puzzle.board[row][col].nw != puzzle.offboard)
                    {
                        if (solchessMode)
                        {
                            KeyValuePair<int, int> f = new KeyValuePair<int, int>(puzzle.board[row][col].nw.r, puzzle.board[row][col].nw.c);
                            fields.Add(f);
                        }
                        else if (v == 'K')
                        {
                            char p = puzzle.board[row][col].nw.piece;
                            if (p == ' ' || p == 'q' || p == 'r' || p == 'b' || p == 'n' || p == 'p')
                            {
                                KeyValuePair<int, int> f = new KeyValuePair<int, int>(puzzle.board[row][col].nw.r, puzzle.board[row][col].nw.c);
                                fields.Add(f);
                            }
                        }
                        else if (v == 'k')
                        {
                            char p = puzzle.board[row][col].nw.piece;
                            if (p == ' ' || p == 'Q' || p == 'R' || p == 'B' || p == 'N' || p == 'P')
                            {
                                KeyValuePair<int, int> f = new KeyValuePair<int, int>(puzzle.board[row][col].nw.r, puzzle.board[row][col].nw.c);
                                fields.Add(f);
                            }
                        }
                    }
                    break;
                case 'N':
                case 'n':
                    if (v == 'n')
                    {
                        if (puzzle.board[row][col].n != puzzle.offboard)
                        {
                            if (puzzle.board[row][col].n.n != puzzle.offboard)
                            {
                                if (puzzle.board[row][col].n.n.e != puzzle.offboard)
                                {
                                    char p = puzzle.board[row][col].n.n.e.piece;
                                    if (p == ' ' || p == 'Q' || p == 'R' || p == 'B' || p == 'N' || p == 'P' )
                                    {
                                        KeyValuePair<int, int> f = new KeyValuePair<int, int>(puzzle.board[row][col].n.n.e.r, puzzle.board[row][col].n.n.e.c);
                                        fields.Add(f);
                                    }
                                }
                                if (puzzle.board[row][col].n.n.w != puzzle.offboard)
                                {
                                    char p = puzzle.board[row][col].n.n.w.piece;
                                    if (p == ' ' || p == 'Q' || p == 'R' || p == 'B' || p == 'N' || p == 'P')
                                    {
                                        KeyValuePair<int, int> f = new KeyValuePair<int, int>(puzzle.board[row][col].n.n.w.r, puzzle.board[row][col].n.n.w.c);
                                        fields.Add(f);
                                    }
                                }
                            }
                            if (puzzle.board[row][col].n.e != puzzle.offboard && puzzle.board[row][col].n.e.e != puzzle.offboard)
                            {
                                char p = puzzle.board[row][col].n.e.e.piece;
                                if (p == ' ' || p == 'Q' || p == 'R' || p == 'B' || p == 'N' || p == 'P')
                                {
                                    KeyValuePair<int, int> f = new KeyValuePair<int, int>(puzzle.board[row][col].n.e.e.r, puzzle.board[row][col].n.e.e.c);
                                    fields.Add(f);
                                }
                            }
                            if (puzzle.board[row][col].n.w != puzzle.offboard && puzzle.board[row][col].n.w.w != puzzle.offboard)
                            {
                                char p = puzzle.board[row][col].n.w.w.piece;
                                if (p == ' ' || p == 'Q' || p == 'R' || p == 'B' || p == 'N' || p == 'P')
                                {
                                    KeyValuePair<int, int> f = new KeyValuePair<int, int>(puzzle.board[row][col].n.w.w.r, puzzle.board[row][col].n.w.w.c);
                                    fields.Add(f);
                                }
                            }
                        }

                        if (puzzle.board[row][col].s != puzzle.offboard)
                        {
                            if (puzzle.board[row][col].s.s != puzzle.offboard)
                            {
                                if (puzzle.board[row][col].s.s.e != puzzle.offboard)
                                {
                                    char p = puzzle.board[row][col].s.s.e.piece;
                                    if (p == ' ' || p == 'Q' || p == 'R' || p == 'B' || p == 'N' || p == 'P')
                                    {
                                        KeyValuePair<int, int> f = new KeyValuePair<int, int>(puzzle.board[row][col].s.s.e.r, puzzle.board[row][col].s.s.e.c);
                                        fields.Add(f);
                                    }
                                }
                                if (puzzle.board[row][col].s.s.w != puzzle.offboard)
                                {
                                    char p = puzzle.board[row][col].s.s.w.piece;
                                    if (p == ' ' || p == 'Q' || p == 'R' || p == 'B' || p == 'N' || p == 'P')
                                    {
                                        KeyValuePair<int, int> f = new KeyValuePair<int, int>(puzzle.board[row][col].s.s.w.r, puzzle.board[row][col].s.s.w.c);
                                        fields.Add(f);
                                    }
                                }
                            }
                            if (puzzle.board[row][col].s.e != puzzle.offboard && puzzle.board[row][col].s.e.e != puzzle.offboard)
                            {
                                char p = puzzle.board[row][col].s.e.e.piece;
                                if (p == ' ' || p == 'Q' || p == 'R' || p == 'B' || p == 'N' || p == 'P')
                                {
                                    KeyValuePair<int, int> f = new KeyValuePair<int, int>(puzzle.board[row][col].s.e.e.r, puzzle.board[row][col].s.e.e.c);
                                    fields.Add(f);
                                }
                            }
                            if (puzzle.board[row][col].s.w != puzzle.offboard && puzzle.board[row][col].s.w.w != puzzle.offboard)
                            {
                                char p = puzzle.board[row][col].s.w.w.piece;
                                if (p == ' ' || p == 'Q' || p == 'R' || p == 'B' || p == 'N' || p == 'P')
                                {
                                    KeyValuePair<int, int> f = new KeyValuePair<int, int>(puzzle.board[row][col].s.w.w.r, puzzle.board[row][col].s.w.w.c);
                                    fields.Add(f);
                                }
                            }
                        }
                    }
                    else if(v == 'N')
                    {
                        if (puzzle.board[row][col].n != puzzle.offboard)
                        {
                            if (puzzle.board[row][col].n.n != puzzle.offboard)
                            {
                                if (puzzle.board[row][col].n.n.e != puzzle.offboard)
                                {
                                    char p = puzzle.board[row][col].n.n.e.piece;
                                    if (p == ' ' || p == 'q' || p == 'r' || p == 'b' || p == 'n' || p == 'p' || (solchessMode && (p == 'Q' || p == 'R' || p == 'B' || p == 'N' || p == 'P')))
                                    {
                                        KeyValuePair<int, int> f = new KeyValuePair<int, int>(puzzle.board[row][col].n.n.e.r, puzzle.board[row][col].n.n.e.c);
                                        fields.Add(f);
                                    }
                                }
                                if (puzzle.board[row][col].n.n.w != puzzle.offboard)
                                {
                                    char p = puzzle.board[row][col].n.n.w.piece;
                                    if (p == ' ' || p == 'q' || p == 'r' || p == 'b' || p == 'n' || p == 'p' || (solchessMode && (p == 'Q' || p == 'R' || p == 'B' || p == 'N' || p == 'P')))
                                    {
                                        KeyValuePair<int, int> f = new KeyValuePair<int, int>(puzzle.board[row][col].n.n.w.r, puzzle.board[row][col].n.n.w.c);
                                        fields.Add(f);
                                    }
                                }
                            }
                            if (puzzle.board[row][col].n.e != puzzle.offboard && puzzle.board[row][col].n.e.e != puzzle.offboard)
                            {
                                char p = puzzle.board[row][col].n.e.e.piece;
                                if (p == ' ' || p == 'q' || p == 'r' || p == 'b' || p == 'n' || p == 'p' || (solchessMode && (p == 'Q' || p == 'R' || p == 'B' || p == 'N' || p == 'P')))
                                {
                                    KeyValuePair<int, int> f = new KeyValuePair<int, int>(puzzle.board[row][col].n.e.e.r, puzzle.board[row][col].n.e.e.c);
                                    fields.Add(f);
                                }
                            }
                            if (puzzle.board[row][col].n.w != puzzle.offboard && puzzle.board[row][col].n.w.w != puzzle.offboard)
                            {
                                char p = puzzle.board[row][col].n.w.w.piece;
                                if (p == ' ' || p == 'q' || p == 'r' || p == 'b' || p == 'n' || p == 'p' || (solchessMode && (p == 'Q' || p == 'R' || p == 'B' || p == 'N' || p == 'P')))
                                {
                                    KeyValuePair<int, int> f = new KeyValuePair<int, int>(puzzle.board[row][col].n.w.w.r, puzzle.board[row][col].n.w.w.c);
                                    fields.Add(f);
                                }
                            }
                        }

                        if (puzzle.board[row][col].s != puzzle.offboard)
                        {
                            if (puzzle.board[row][col].s.s != puzzle.offboard)
                            {
                                if (puzzle.board[row][col].s.s.e != puzzle.offboard)
                                {
                                    char p = puzzle.board[row][col].s.s.e.piece;
                                    if (p == ' ' || p == 'q' || p == 'r' || p == 'b' || p == 'n' || p == 'p' || (solchessMode && (p == 'Q' || p == 'R' || p == 'B' || p == 'N' || p == 'P')))
                                    {
                                        KeyValuePair<int, int> f = new KeyValuePair<int, int>(puzzle.board[row][col].s.s.e.r, puzzle.board[row][col].s.s.e.c);
                                        fields.Add(f);
                                    }
                                }
                                if (puzzle.board[row][col].s.s.w != puzzle.offboard)
                                {
                                    char p = puzzle.board[row][col].s.s.w.piece;
                                    if (p == ' ' || p == 'q' || p == 'r' || p == 'b' || p == 'n' || p == 'p' || (solchessMode && (p == 'Q' || p == 'R' || p == 'B' || p == 'N' || p == 'P')))
                                    {
                                        KeyValuePair<int, int> f = new KeyValuePair<int, int>(puzzle.board[row][col].s.s.w.r, puzzle.board[row][col].s.s.w.c);
                                        fields.Add(f);
                                    }
                                }
                            }
                            if (puzzle.board[row][col].s.e != puzzle.offboard && puzzle.board[row][col].s.e.e != puzzle.offboard)
                            {
                                char p = puzzle.board[row][col].s.e.e.piece;
                                if (p == ' ' || p == 'q' || p == 'r' || p == 'b' || p == 'n' || p == 'p' || (solchessMode && (p == 'Q' || p == 'R' || p == 'B' || p == 'N' || p == 'P')))
                                {
                                    KeyValuePair<int, int> f = new KeyValuePair<int, int>(puzzle.board[row][col].s.e.e.r, puzzle.board[row][col].s.e.e.c);
                                    fields.Add(f);
                                }
                            }
                            if (puzzle.board[row][col].s.w != puzzle.offboard && puzzle.board[row][col].s.w.w != puzzle.offboard)
                            {
                                char p = puzzle.board[row][col].s.w.w.piece;
                                if (p == ' ' || p == 'q' || p == 'r' || p == 'b' || p == 'n' || p == 'p' || (solchessMode && (p == 'Q' || p == 'R' || p == 'B' || p == 'N' || p == 'P')))
                                {
                                    KeyValuePair<int, int> f = new KeyValuePair<int, int>(puzzle.board[row][col].s.w.w.r, puzzle.board[row][col].s.w.w.c);
                                    fields.Add(f);
                                }
                            }
                        }
                    }


                    break;
                case 'B':
                case 'b': 
                    currentField = puzzle.board[row][col].ne;
                    while (currentField != puzzle.offboard && currentField.piece == ' ')
                    {
                        KeyValuePair<int, int> p = new KeyValuePair<int, int>(currentField.r, currentField.c);
                        fields.Add(p);
                        currentField = currentField.ne;
                    }
                    if (currentField != puzzle.offboard)
                    {
                        if ((v == 'B' && currentField.piece == 'k' || currentField.piece == 'q' || currentField.piece == 'r' ||
                            currentField.piece == 'b' || currentField.piece == 'n' || currentField.piece == 'p') || (v == 'B' && solchessMode))
                        {
                            KeyValuePair<int, int> p = new KeyValuePair<int, int>(currentField.r, currentField.c);
                            fields.Add(p);
                        }
                        else if (v == 'b' && currentField.piece == 'K' || currentField.piece == 'Q' || currentField.piece == 'R' ||
                            currentField.piece == 'B' || currentField.piece == 'N' || currentField.piece == 'P')
                        {
                            KeyValuePair<int, int> p = new KeyValuePair<int, int>(currentField.r, currentField.c);
                            fields.Add(p);
                        }

                    }
                    currentField = puzzle.board[row][col].se;
                    while (currentField != puzzle.offboard && currentField.piece == ' ')
                    {
                        KeyValuePair<int, int> p = new KeyValuePair<int, int>(currentField.r, currentField.c);
                        fields.Add(p);
                        currentField = currentField.se;
                    }
                    if (currentField != puzzle.offboard)
                    {
                        if ((v == 'B' && currentField.piece == 'k' || currentField.piece == 'q' || currentField.piece == 'r' ||
                            currentField.piece == 'b' || currentField.piece == 'n' || currentField.piece == 'p') || (v == 'B' && solchessMode))
                        {
                            KeyValuePair<int, int> p = new KeyValuePair<int, int>(currentField.r, currentField.c);
                            fields.Add(p);
                        }
                        else if (v == 'b' && currentField.piece == 'K' || currentField.piece == 'Q' || currentField.piece == 'R' ||
                            currentField.piece == 'B' || currentField.piece == 'N' || currentField.piece == 'P')
                        {
                            KeyValuePair<int, int> p = new KeyValuePair<int, int>(currentField.r, currentField.c);
                            fields.Add(p);
                        }

                    }

                    currentField = puzzle.board[row][col].sw;
                    while (currentField != puzzle.offboard && currentField.piece == ' ')
                    {
                        KeyValuePair<int, int> p = new KeyValuePair<int, int>(currentField.r, currentField.c);
                        fields.Add(p);
                        currentField = currentField.sw;
                    }
                    if (currentField != puzzle.offboard)
                    {
                        if ((v == 'B' && currentField.piece == 'k' || currentField.piece == 'q' || currentField.piece == 'r' ||
                            currentField.piece == 'b' || currentField.piece == 'n' || currentField.piece == 'p') || (v == 'B' && solchessMode))
                        {
                            KeyValuePair<int, int> p = new KeyValuePair<int, int>(currentField.r, currentField.c);
                            fields.Add(p);
                        }
                        else if (v == 'b' && currentField.piece == 'K' || currentField.piece == 'Q' || currentField.piece == 'R' ||
                            currentField.piece == 'B' || currentField.piece == 'N' || currentField.piece == 'P')
                        {
                            KeyValuePair<int, int> p = new KeyValuePair<int, int>(currentField.r, currentField.c);
                            fields.Add(p);
                        }

                    }
                    currentField = puzzle.board[row][col].nw;
                    while (currentField != puzzle.offboard && currentField.piece == ' ')
                    {
                        KeyValuePair<int, int> p = new KeyValuePair<int, int>(currentField.r, currentField.c);
                        fields.Add(p);
                        currentField = currentField.nw;
                    }
                    if (currentField != puzzle.offboard)
                    {
                        if ((v == 'B' && currentField.piece == 'k' || currentField.piece == 'q' || currentField.piece == 'r' ||
                            currentField.piece == 'b' || currentField.piece == 'n' || currentField.piece == 'p') || (v == 'B' && solchessMode))
                        {
                            KeyValuePair<int, int> p = new KeyValuePair<int, int>(currentField.r, currentField.c);
                            fields.Add(p);
                        }
                        else if (v == 'b' && currentField.piece == 'K' || currentField.piece == 'Q' || currentField.piece == 'R' ||
                            currentField.piece == 'B' || currentField.piece == 'N' || currentField.piece == 'P')
                        {
                            KeyValuePair<int, int> p = new KeyValuePair<int, int>(currentField.r, currentField.c);
                            fields.Add(p);
                        }

                    }
                    break;
                case 'R':
                case 'r':
                    currentField = puzzle.board[row][col].n;
                    while (currentField != puzzle.offboard && currentField.piece == ' ')
                    {
                        KeyValuePair<int, int> p = new KeyValuePair<int, int>(currentField.r, currentField.c);
                        fields.Add(p);
                        currentField = currentField.n;
                    }
                    if (currentField != puzzle.offboard)
                    {
                        if ((v == 'R' && currentField.piece == 'k' || currentField.piece == 'q' || currentField.piece == 'r' ||
                            currentField.piece == 'b' || currentField.piece == 'n' || currentField.piece == 'p') || (v == 'R' && solchessMode))
                        {
                            KeyValuePair<int, int> p = new KeyValuePair<int, int>(currentField.r, currentField.c);
                            fields.Add(p);
                        }
                        else if (v == 'r' && currentField.piece == 'K' || currentField.piece == 'Q' || currentField.piece == 'R' ||
                            currentField.piece == 'B' || currentField.piece == 'N' || currentField.piece == 'P')
                        {
                            KeyValuePair<int, int> p = new KeyValuePair<int, int>(currentField.r, currentField.c);
                            fields.Add(p);
                        }

                    }
                    currentField = puzzle.board[row][col].e;
                    while (currentField != puzzle.offboard && currentField.piece == ' ')
                    {
                        KeyValuePair<int, int> p = new KeyValuePair<int, int>(currentField.r, currentField.c);
                        fields.Add(p);
                        currentField = currentField.e;
                    }
                    if (currentField != puzzle.offboard)
                    {
                        if ((v == 'R' && currentField.piece == 'k' || currentField.piece == 'q' || currentField.piece == 'r' ||
                            currentField.piece == 'b' || currentField.piece == 'n' || currentField.piece == 'p') || (v == 'R' && solchessMode))
                        {
                            KeyValuePair<int, int> p = new KeyValuePair<int, int>(currentField.r, currentField.c);
                            fields.Add(p);
                        }
                        else if (v == 'r' && currentField.piece == 'K' || currentField.piece == 'Q' || currentField.piece == 'R' ||
                            currentField.piece == 'B' || currentField.piece == 'N' || currentField.piece == 'P')
                        {
                            KeyValuePair<int, int> p = new KeyValuePair<int, int>(currentField.r, currentField.c);
                            fields.Add(p);
                        }

                    }
                    currentField = puzzle.board[row][col].s;
                    while (currentField != puzzle.offboard && currentField.piece == ' ')
                    {
                        KeyValuePair<int, int> p = new KeyValuePair<int, int>(currentField.r, currentField.c);
                        fields.Add(p);
                        currentField = currentField.s;
                    }
                    if (currentField != puzzle.offboard)
                    {
                        if ((v == 'R' && currentField.piece == 'k' || currentField.piece == 'q' || currentField.piece == 'r' ||
                            currentField.piece == 'b' || currentField.piece == 'n' || currentField.piece == 'p') || (v == 'R' && solchessMode))
                        {
                            KeyValuePair<int, int> p = new KeyValuePair<int, int>(currentField.r, currentField.c);
                            fields.Add(p);
                        }
                        else if (v == 'r' && currentField.piece == 'K' || currentField.piece == 'Q' || currentField.piece == 'R' ||
                            currentField.piece == 'B' || currentField.piece == 'N' || currentField.piece == 'P')
                        {
                            KeyValuePair<int, int> p = new KeyValuePair<int, int>(currentField.r, currentField.c);
                            fields.Add(p);
                        }

                    }
                    currentField = puzzle.board[row][col].w;
                    while (currentField != puzzle.offboard && currentField.piece == ' ')
                    {
                        KeyValuePair<int, int> p = new KeyValuePair<int, int>(currentField.r, currentField.c);
                        fields.Add(p);
                        currentField = currentField.w;
                    }
                    if (currentField != puzzle.offboard)
                    {
                        if ((v == 'R' && currentField.piece == 'k' || currentField.piece == 'q' || currentField.piece == 'r' ||
                            currentField.piece == 'b' || currentField.piece == 'n' || currentField.piece == 'p') || (v == 'R' && solchessMode))
                        {
                            KeyValuePair<int, int> p = new KeyValuePair<int, int>(currentField.r, currentField.c);
                            fields.Add(p);
                        }
                        else if (v == 'r' && currentField.piece == 'K' || currentField.piece == 'Q' || currentField.piece == 'R' ||
                            currentField.piece == 'B' || currentField.piece == 'N' || currentField.piece == 'P')
                        {
                            KeyValuePair<int, int> p = new KeyValuePair<int, int>(currentField.r, currentField.c);
                            fields.Add(p);
                        }

                    }
                    break;
            }

            foreach(KeyValuePair<int, int> f in fields)
            {
                if(solchessMode)
                {
                    if(puzzle.board[f.Key][f.Value].piece == ' ')
                    {
                        continue;
                    }
                }
                rects[f.Key][f.Value].Fill = new SolidColorBrush(Colors.Green);
            }
        }

        

        public void SetPuzzle(string puzz)
        {
            CleanUp();
            originalPuzzleRepresentation = puzz;
            puzzle = new Puzzle();
            puzzle.pieceCount = 0;
            if (puzz.Length >= 16)
            {
                for (int i = 0; i < Puzzle.SIZE; i++)
                {
                    for (int j = 0; j < Puzzle.SIZE; j++)
                    {
                        switch (puzz[i * 4 + j])
                        {
                            case 'P':
                            case 'Q':
                            case 'K':
                            case 'N':
                            case 'R':
                            case 'B':
                                puzzle.pieceCount++;
                                puzzle.board[i][j].piece = puzz[i * 4 + j];
                                break;
                            case '-':
                                continue;
                            default:
                                return;
                        }
                    }
                }
            }
            ParseAndDisplayPuzzle();
        }

        private void ParseAndDisplayPuzzle()
        {
            for(int i=0; i<Puzzle.SIZE; i++)
            {
                for(int j=0; j<Puzzle.SIZE; j++)
                {
                    if(puzzle.board[i][j].piece != '-')
                    {
                        switch(puzzle.board[i][j].piece)
                        {
                            case 'Q':
                                SpawnQueen(i, j);
                                break;
                            case 'P':
                                SpawnPawn(i, j);
                                break;
                            case 'N':
                                SpawnKnight(i, j);
                                break;
                            case 'B':
                                SpawnBishop(i, j);
                                break;
                            case 'R':
                                SpawnRook(i, j);
                                break;
                            case 'K':
                                SpawnKing(i, j);
                                break;
                        }
                    }

                }
            }
        }

        public void ShowHint()
        {
            hintsUsed = true;
            String s = "";
            for(int i=0; i<Puzzle.SIZE; i++)
            {
                for(int j=0; j<Puzzle.SIZE; j++)
                {
                    s += puzzle.board[i][j].piece != ' ' ? puzzle.board[i][j].piece : '-';
                }
            }
            Solver solver = new Solver();
            int sols = solver.InitAndRun(s);
            if(sols > 0)
            {
                Move m = solver.root;
                while (m.previous != null && m.previous.previous != null)
                {
                    m = m.previous;
                }
                blinkSquares(m.r1, m.c1, m.r2, m.c2);
            }
            else
            {

            }
        }

        private void blinkSquares(int r1, int c1, int r2, int c2)
        {
            rects[r1][c1].Fill = new SolidColorBrush(Colors.Orange);
            rects[r2][c2].Fill = new SolidColorBrush(Colors.Red);
            Unblink(r1, c1, r2, c2);
        }

        public async Task Unblink(int r1, int c1, int r2, int c2)
        {
            await Task.Delay(2000);
            rects[r1][c1].Fill = GetFillColor(r1, c1);
            rects[r2][c2].Fill = GetFillColor(r2, c2);
        }

        private void SpawnPieces(char p, int i, int j)
        {
            string url = GetThemedPieceImage(p);
            Image piece = new Image
            {
                Source = new BitmapImage(new Uri("ms-appx:///" + url, UriKind.Absolute)),
                CanDrag = true,
                AllowDrop = true,
                Height = 80,
                Width = 80,
                Stretch = Stretch.None,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY,
                RenderTransform = new CompositeTransform()
            };

            Grid.SetRow(piece, i);
            Grid.SetColumn(piece, j);
            BoardGrid.Children.Add(piece);
            if (IsMobile)
            {
                piece.ManipulationDelta += delegate (object sender, ManipulationDeltaRoutedEventArgs e)
                {
                    var transform = (CompositeTransform)piece.RenderTransform;
                    transform.TranslateX += e.Delta.Translation.X;
                    transform.TranslateY += e.Delta.Translation.Y;
                };
            }
            piece.PointerPressed += delegate (object sender, PointerRoutedEventArgs e)
            {
                currentImage = piece;
                int row = images[piece].Key;
                int col = images[piece].Value;
                Canvas.SetZIndex(piece, MaxZIndex());
                ShowPossibilities(row, col, p);
            };
            piece.PointerReleased += delegate (object sender, PointerRoutedEventArgs e)
            {
                if (IsMobile)
                {
                    IEnumerable<UIElement> list = VisualTreeHelper.FindElementsInHostCoordinates(e.GetCurrentPoint(this).Position, this);
                    foreach (UIElement el in list)
                    {
                        if (el is Image)
                        {
                            Image img = el as Image;
                            if (img != piece)
                            {
                                KeyValuePair<int, int> pos = images[img];
                                RouteDrop(pos.Key, pos.Value, sender, null);
                                HidePossibilities();
                                return;
                            }
                        }
                    }
                    KeyValuePair<int, int> oldPos = images[currentImage];
                    Grid.SetRow(currentImage, oldPos.Key);
                    Grid.SetColumn(currentImage, oldPos.Value);

                    currentImage.RenderTransform = new CompositeTransform();

                }
                HidePossibilities();
            };
            piece.DragStarting += delegate (UIElement e, DragStartingEventArgs a) {
                currentImage = piece;
                int row = images[piece].Key;
                int col = images[piece].Value;
                ShowPossibilities(row, col, p);
            };
            piece.Drop += delegate (object sender, DragEventArgs e)
            {
                KeyValuePair<int, int> pair = images[piece];
                RouteDropToRect(pair.Key, pair.Value, sender, e);
            };
            piece.DropCompleted += delegate (UIElement e, DropCompletedEventArgs a)
            {
                currentImage.Visibility = Visibility.Visible;
                HidePossibilities();
                currentImage = null;

            };
            piece.PointerReleased += delegate (object sender, PointerRoutedEventArgs e)
            {
                HidePossibilities();
            };
            images.Add(piece, new KeyValuePair<int, int>(i, j));
        }

        private string GetThemedPieceImage(char p)
        {
            throw new NotImplementedException();
        }

        public void GridDragOverCustomized(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Move;
            e.DragUIOverride.IsCaptionVisible = false; // Sets if the caption is visible
            e.DragUIOverride.IsContentVisible = true; // Sets if the dragged content is visible
            e.DragUIOverride.IsGlyphVisible = false; // Sets if the glyph is visibile
        }

        private void SpawnKing(int i, int j)
        {
            Image king = new Image
            {
                Source = new BitmapImage(new Uri("ms-appx:///Assets/merida/WhiteKing.png", UriKind.Absolute)),
                CanDrag = true,
                AllowDrop = true,
                Height = 80,
                Width = 80,
                Stretch = Stretch.None,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY,
                RenderTransform = new CompositeTransform()

            };

            Grid.SetRow(king, i);
            Grid.SetColumn(king, j);
            BoardGrid.Children.Add(king);
            if (IsMobile)
            {
                king.ManipulationDelta += delegate (object sender, ManipulationDeltaRoutedEventArgs e)
                {
                    var transform = (CompositeTransform)king.RenderTransform;
                    transform.TranslateX += e.Delta.Translation.X;
                    transform.TranslateY += e.Delta.Translation.Y;
                };
            }
            king.PointerPressed += delegate (object sender, PointerRoutedEventArgs e)
            {
                currentImage = king;
                int row = images[king].Key;
                int col = images[king].Value;
                Canvas.SetZIndex(king, MaxZIndex());
                ShowPossibilities(row, col, 'K');
            };
            king.PointerReleased += delegate (object sender, PointerRoutedEventArgs e)
            {
                if (IsMobile)
                {
                    IEnumerable<UIElement> list = VisualTreeHelper.FindElementsInHostCoordinates(e.GetCurrentPoint(this).Position, this);
                    foreach (UIElement el in list)
                    {
                        if (el is Image)
                        {
                            Image img = el as Image;
                            if (img != king)
                            {
                                KeyValuePair<int, int> pos = images[img];
                                RouteDrop(pos.Key, pos.Value, sender, null);
                                HidePossibilities();
                                return;
                            }
                        }
                    }
                    KeyValuePair<int, int> oldPos = images[currentImage];
                    Grid.SetRow(currentImage, oldPos.Key);
                    Grid.SetColumn(currentImage, oldPos.Value);
                    currentImage.RenderTransform = new CompositeTransform();

                }
                HidePossibilities();
            };
            king.DragStarting += delegate(UIElement e, DragStartingEventArgs a) {
                currentImage = king;
                int row = images[king].Key;
                int col = images[king].Value;
                ShowPossibilities(row, col, 'K');
            };
            king.Drop += delegate (object sender, DragEventArgs e)
            {
                KeyValuePair<int, int> p = images[king];
                RouteDrop(p.Key, p.Value, sender, e);
            };
            king.DropCompleted += delegate (UIElement e, DropCompletedEventArgs a)
            {
                currentImage.Visibility = Visibility.Visible;
                HidePossibilities();
                currentImage = null;
                
            };
            images.Add(king, new KeyValuePair<int, int>(i, j));
        }

        

        private void SpawnRook(int i, int j)
        {
            Image rook = new Image
            {
                Source = new BitmapImage(new Uri("ms-appx:///Assets/merida/WhiteRook.png")),
                CanDrag = true,
                AllowDrop = true,
                Height = 80,
                Width = 80,
                Stretch = Stretch.None,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY,
                RenderTransform = new CompositeTransform()
            };

            Grid.SetRow(rook, i);
            Grid.SetColumn(rook, j);
            BoardGrid.Children.Add(rook);
            if (IsMobile)
            {
                rook.ManipulationDelta += delegate (object sender, ManipulationDeltaRoutedEventArgs e)
                {
                    var transform = (CompositeTransform)rook.RenderTransform;
                    transform.TranslateX += e.Delta.Translation.X;
                    transform.TranslateY += e.Delta.Translation.Y;
                };
            }
            rook.PointerPressed += delegate (object sender, PointerRoutedEventArgs e)
            {
                currentImage = rook;
                int row = images[rook].Key;
                int col = images[rook].Value;
                Canvas.SetZIndex(rook, MaxZIndex());
                ShowPossibilities(row, col, 'R');
            };
            rook.PointerReleased += delegate (object sender, PointerRoutedEventArgs e)
            {
                if (IsMobile)
                {
                    IEnumerable<UIElement> list = VisualTreeHelper.FindElementsInHostCoordinates(e.GetCurrentPoint(this).Position, this);
                    foreach (UIElement el in list)
                    {
                        if (el is Image)
                        {
                            Image img = el as Image;
                            if (img != rook)
                            {
                                KeyValuePair<int, int> pos = images[img];
                                RouteDrop(pos.Key, pos.Value, sender, null);
                                HidePossibilities();
                                return;
                            }
                        }
                    }
                    KeyValuePair<int, int> oldPos = images[currentImage];
                    Grid.SetRow(currentImage, oldPos.Key);
                    Grid.SetColumn(currentImage, oldPos.Value);
                    currentImage.RenderTransform = new CompositeTransform();
                }
                HidePossibilities();
            };
            rook.DragStarting += delegate (UIElement e, DragStartingEventArgs a) {
                currentImage = rook;
                int row = images[rook].Key;
                int col = images[rook].Value;
                ShowPossibilities(row, col, 'R');
            };
            rook.Drop += delegate (object sender, DragEventArgs e)
            {
                KeyValuePair<int, int> p = images[rook];
                RouteDrop(p.Key, p.Value, sender, e);
            };
            rook.DropCompleted += delegate (UIElement e, DropCompletedEventArgs a)
            {
                currentImage.Visibility = Visibility.Visible;
                HidePossibilities();
                currentImage = null;

            };
            images.Add(rook, new KeyValuePair<int, int>(i, j));
        }

        private void SpawnBishop(int i, int j)
        {
            Image bishop = new Image
            {
                Source = new BitmapImage(new Uri("ms-appx:///Assets/merida/WhiteBishop.png")),
                CanDrag = true,
                AllowDrop = true,
                Height = 80,
                Width = 80,
                Stretch = Stretch.None,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY,
                RenderTransform = new CompositeTransform()
            };
            Grid.SetRow(bishop, i);
            Grid.SetColumn(bishop, j);
            BoardGrid.Children.Add(bishop);
            if (IsMobile)
            {
                bishop.ManipulationDelta += delegate (object sender, ManipulationDeltaRoutedEventArgs e)
                {
                    var transform = (CompositeTransform)bishop.RenderTransform;
                    transform.TranslateX += e.Delta.Translation.X;
                    transform.TranslateY += e.Delta.Translation.Y;
                };
            }
            bishop.PointerPressed += delegate (object sender, PointerRoutedEventArgs e)
            {
                currentImage = bishop;
                int row = images[bishop].Key;
                int col = images[bishop].Value;
                Canvas.SetZIndex(bishop, MaxZIndex());
                ShowPossibilities(row, col, 'B');
            };
            bishop.PointerReleased += delegate (object sender, PointerRoutedEventArgs e)
            {
                if (IsMobile)
                {
                    IEnumerable<UIElement> list = VisualTreeHelper.FindElementsInHostCoordinates(e.GetCurrentPoint(this).Position, this);
                    foreach (UIElement el in list)
                    {
                        if (el is Image)
                        {
                            Image img = el as Image;
                            if (img != bishop)
                            {
                                KeyValuePair<int, int> pos = images[img];
                                RouteDrop(pos.Key, pos.Value, sender, null);
                                HidePossibilities();
                                return;
                            }
                        }
                    }
                    KeyValuePair<int, int> oldPos = images[currentImage];
                    Grid.SetRow(currentImage, oldPos.Key);
                    Grid.SetColumn(currentImage, oldPos.Value);
                    currentImage.RenderTransform = new CompositeTransform();
                }
                HidePossibilities();
            };
            bishop.DragStarting += delegate (UIElement e, DragStartingEventArgs a) {
                currentImage = bishop;
                int row = images[bishop].Key;
                int col = images[bishop].Value;
                ShowPossibilities(row, col, 'B');
            };
            bishop.Drop += delegate (object sender, DragEventArgs e)
            {
                KeyValuePair<int, int> p = images[bishop];
                RouteDrop(p.Key, p.Value, sender, e);
            };
            bishop.DropCompleted += delegate (UIElement e, DropCompletedEventArgs a)
            {
                currentImage.Visibility = Visibility.Visible;
                HidePossibilities();
                currentImage = null;

            };
            images.Add(bishop, new KeyValuePair<int, int>(i, j));
        }

        private void SpawnKnight(int i, int j)
        {
            Image knight = new Image
            {
                Source = new BitmapImage(new Uri("ms-appx:///Assets/merida/WhiteKnight.png")),
                CanDrag = true,
                AllowDrop = true,
                Height = 80,
                Width = 80,
                Stretch = Stretch.None,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY,
                RenderTransform = new CompositeTransform()
            };
            Grid.SetRow(knight, i);
            Grid.SetColumn(knight, j);
            BoardGrid.Children.Add(knight);

            if (IsMobile)
            {
                knight.ManipulationDelta += delegate (object sender, ManipulationDeltaRoutedEventArgs e)
                {
                    var transform = (CompositeTransform)knight.RenderTransform;
                    transform.TranslateX += e.Delta.Translation.X;
                    transform.TranslateY += e.Delta.Translation.Y;
                };
            }
            knight.PointerPressed += delegate (object sender, PointerRoutedEventArgs e)
            {
                currentImage = knight;
                int row = images[knight].Key;
                int col = images[knight].Value;
                Canvas.SetZIndex(knight, MaxZIndex());
                ShowPossibilities(row, col, 'N');
            };
            knight.PointerReleased += delegate (object sender, PointerRoutedEventArgs e)
            {
                if (IsMobile)
                {
                    IEnumerable<UIElement> list = VisualTreeHelper.FindElementsInHostCoordinates(e.GetCurrentPoint(this).Position, this);
                    foreach (UIElement el in list)
                    {
                        if (el is Image)
                        {
                            Image img = el as Image;
                            if (img != knight)
                            {
                                KeyValuePair<int, int> pos = images[img];
                                RouteDrop(pos.Key, pos.Value, sender, null);
                                HidePossibilities();
                                return;
                            }
                        }
                    }
                    KeyValuePair<int, int> oldPos = images[currentImage];
                    Grid.SetRow(currentImage, oldPos.Key);
                    Grid.SetColumn(currentImage, oldPos.Value);
                    currentImage.RenderTransform = new CompositeTransform();
                }
                HidePossibilities();
            };
            knight.DragStarting += delegate (UIElement e, DragStartingEventArgs a) {
                currentImage = knight;
                int row = images[knight].Key;
                int col = images[knight].Value;
                ShowPossibilities(row, col, 'N');
            };
            knight.Drop += delegate (object sender, DragEventArgs e)
            {
                KeyValuePair<int, int> p = images[knight];
                RouteDrop(p.Key, p.Value, sender, e);
            };
            knight.DropCompleted += delegate (UIElement e, DropCompletedEventArgs a)
            {
                currentImage.Visibility = Visibility.Visible;
                HidePossibilities();
                currentImage = null;

            };
            images.Add(knight, new KeyValuePair<int, int>(i, j));
        }

        public static bool IsMobile
        {
            get
            {
                var qualifiers = Windows.ApplicationModel.Resources.Core.ResourceContext.GetForCurrentView().QualifierValues;
                return (qualifiers.ContainsKey("DeviceFamily") && qualifiers["DeviceFamily"] == "Mobile");
            }
        }

        private void SpawnPawn(int i, int j)
        {
            Image pawn = new Image
            {
                Source = new BitmapImage(new Uri("ms-appx:///Assets/merida/WhitePawn.png")),
                CanDrag = true,
                AllowDrop = true,
                Height = 80,
                Width = 80,
                Stretch = Stretch.None,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY,
                RenderTransform = new CompositeTransform()
            };

            Grid.SetRow(pawn, i);
            Grid.SetColumn(pawn, j);
            BoardGrid.Children.Add(pawn);

            if (IsMobile)
            {
                pawn.ManipulationDelta += delegate (object sender, ManipulationDeltaRoutedEventArgs e)
                {
                    var transform = (CompositeTransform)pawn.RenderTransform;
                    transform.TranslateX += e.Delta.Translation.X;
                    transform.TranslateY += e.Delta.Translation.Y;
                };
            }

            pawn.PointerPressed += delegate (object sender, PointerRoutedEventArgs e)
            {
                currentImage = pawn;
                int row = images[pawn].Key;
                int col = images[pawn].Value;
                Canvas.SetZIndex(pawn, MaxZIndex());
                ShowPossibilities(row, col, 'P');
                e.Handled = true;
            };
            pawn.PointerReleased += delegate (object sender, PointerRoutedEventArgs e)
            {
                if(IsMobile)
                {
                    IEnumerable<UIElement> list = VisualTreeHelper.FindElementsInHostCoordinates(e.GetCurrentPoint(this).Position, this);
                    foreach(UIElement el in list)
                    {
                        if(el is Image)
                        {
                            Image img = el as Image;
                            if(img != pawn)
                            {
                                KeyValuePair<int, int> pos = images[img];
                                RouteDrop(pos.Key, pos.Value, sender, null);
                                HidePossibilities();
                                return;
                            }
                        }
                    }
                    KeyValuePair<int, int> oldPos = images[currentImage];
                    Grid.SetRow(currentImage, oldPos.Key);
                    Grid.SetColumn(currentImage, oldPos.Value);
                    currentImage.RenderTransform = new CompositeTransform();
                }
                HidePossibilities();
            };
            pawn.DragStarting += delegate (UIElement e, DragStartingEventArgs a) {
                a.Data.RequestedOperation = DataPackageOperation.Move;
                currentImage = pawn;
                int row = images[pawn].Key;
                int col = images[pawn].Value;
                ShowPossibilities(row, col, 'P');
            };
            pawn.Drop += delegate (object sender, DragEventArgs e)
            {
                KeyValuePair<int, int> p = images[pawn];
                RouteDrop(p.Key, p.Value, sender, e);
                e.Handled = true;
            };
            pawn.DropCompleted += delegate (UIElement e, DropCompletedEventArgs a)
            {
                currentImage.Visibility = Visibility.Visible;
                HidePossibilities();
                currentImage = null;

            };
            images.Add(pawn, new KeyValuePair<int, int>(i, j));
        }

        private void RouteDropToRect(int key, int value, object sender, DragEventArgs args)
        {
        }

        private void RouteDrop(int key, int value, object sender, DragEventArgs e)
        {
            if(currentImage == null)
            {
                return;
            }
            if(solchessMode && puzzle.board[key][value].piece == ' ')
            {
                return;
            }
            if(images[currentImage].Key == key && images[currentImage].Value  == value)
            {
                Grid.SetColumn(currentImage, value);
                Grid.SetRow(currentImage, key);
                currentImage.RenderTransform = new CompositeTransform();
                return;
            }
            switch(value)
            {
                case 0: 
                    switch(key)
                    {
                        case 0:
                            a4_Drop(sender, e);
                            break;
                        case 1:
                            a3_Drop(sender, e);
                            break;
                        case 2:
                            a2_Drop(sender, e);
                            break;
                        case 3:
                            a1_Drop(sender, e);
                            break;
                    }
                    break;
                case 1:
                    switch (key)
                    {
                        case 0:
                            b4_Drop(sender, e);
                            break;
                        case 1:
                            b3_Drop(sender, e);
                            break;
                        case 2:
                            b2_Drop(sender, e);
                            break;
                        case 3:
                            b1_Drop(sender, e);
                            break;
                    }
                    break;
                case 2:
                    switch (key)
                    {
                        case 0:
                            c4_Drop(sender, e);
                            break;
                        case 1:
                            c3_Drop(sender, e);
                            break;
                        case 2:
                            c2_Drop(sender, e);
                            break;
                        case 3:
                            c1_Drop(sender, e);
                            break;
                    }
                    break;
                case 3:
                    switch (key)
                    {
                        case 0:
                            d4_Drop(sender, e);
                            break;
                        case 1:
                            d3_Drop(sender, e);
                            break;
                        case 2:
                            d2_Drop(sender, e);
                            break;
                        case 3:
                            d1_Drop(sender, e);
                            break;
                    }
                    break;
            }
            if(currentImage != null && IsMobile)
            {
                KeyValuePair<int, int> pos = images[currentImage];
                Grid.SetColumn(currentImage, pos.Value);
                Grid.SetRow(currentImage, pos.Key);
                currentImage.RenderTransform = new CompositeTransform();
            }
            After_Drop();
        }

        private async void After_Drop()
        {
            if(IsFinished())
            {
                var messageDialog = new MessageDialog("Congratulations. Puzzle solved.");

                // Add commands and set their callbacks; both buttons use the same callback function instead of inline event handlers
                messageDialog.Commands.Add(new UICommand(
                    "Next",
                    new UICommandInvokedHandler(this.CommandInvokedHandler)));
                messageDialog.Commands.Add(new UICommand(
                    "Main Menu",
                    new UICommandInvokedHandler(this.CommandInvokedHandler)));

                // Set the command that will be invoked by default
                messageDialog.DefaultCommandIndex = 0;

                // Set the command to be invoked when escape is pressed
                messageDialog.CancelCommandIndex = 1;

                // Show the message dialog
                await messageDialog.ShowAsync();
            }
        }

        private void CommandInvokedHandler(IUICommand command)
        {
            if (command.Label == "Next")
            {
                CleanUp();
                Controller.GetInstance().SelectNextPuzzle();
            }
            else if(command.Label == "Main Menu")
            {

            }
            
        }

        private void SpawnQueen(int i, int j)
        {
            Image queen = new Image
            {
                Source = new BitmapImage(new Uri("ms-appx:///Assets/merida/WhiteQueen.png")),
                CanDrag = true,
                AllowDrop = true,
                Height = 80,
                Width = 80,
                Stretch = Stretch.None,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY,
                RenderTransform = new CompositeTransform()
            };
            Grid.SetRow(queen, i);
            Grid.SetColumn(queen, j);
            BoardGrid.Children.Add(queen);
            if (IsMobile)
            {
                queen.ManipulationDelta += delegate (object sender, ManipulationDeltaRoutedEventArgs e)
                {
                    var transform = (CompositeTransform)queen.RenderTransform;
                    transform.TranslateX += e.Delta.Translation.X;
                    transform.TranslateY += e.Delta.Translation.Y;
                };
            }
            queen.PointerPressed += delegate (object sender, PointerRoutedEventArgs e)
            {
                currentImage = queen;
                int row = images[queen].Key;
                int col = images[queen].Value;
                Canvas.SetZIndex(queen, MaxZIndex());
                ShowPossibilities(row, col, 'Q');

            };
            queen.PointerReleased += delegate (object sender, PointerRoutedEventArgs e)
            {
                if (IsMobile)
                {
                    IEnumerable<UIElement> list = VisualTreeHelper.FindElementsInHostCoordinates(e.GetCurrentPoint(this).Position, this);
                    foreach (UIElement el in list)
                    {
                        if (el is Image)
                        {
                            Image img = el as Image;
                            if (img != queen)
                            {
                                KeyValuePair<int, int> pos = images[img];
                                RouteDrop(pos.Key, pos.Value, sender, null);
                                HidePossibilities();
                                return;
                            }
                        }
                    }
                    KeyValuePair<int, int> oldPos = images[currentImage];
                    Grid.SetRow(currentImage, oldPos.Key);
                    Grid.SetColumn(currentImage, oldPos.Value);
                    currentImage.RenderTransform = new CompositeTransform();

                }
                HidePossibilities();
            };
            queen.DragStarting += delegate (UIElement e, DragStartingEventArgs a) {
                currentImage = queen;
                int row = images[queen].Key;
                int col = images[queen].Value;
                ShowPossibilities(row, col, 'Q');
            };
            queen.Drop += delegate (object sender, DragEventArgs e)
            {
                KeyValuePair<int, int> p = images[queen];
                RouteDrop(p.Key, p.Value, sender, e);
            };
            queen.DropCompleted += delegate (UIElement e, DropCompletedEventArgs a)
            {
                currentImage.Visibility = Visibility.Visible;
                HidePossibilities();
                currentImage = null;

            };

            images.Add(queen, new KeyValuePair<int, int>(i, j));
        }


        private int MaxZIndex()
        {
            int iMax = 0;
            foreach (UIElement element in BoardGrid.Children)
            {
                int iZIndex = Canvas.GetZIndex(element);
                if (iZIndex > iMax)
                {
                    iMax = iZIndex;
                }
            }
            return iMax + 1;
        }

        public void ResetToBeginning()
        {
            SetPuzzle(originalPuzzleRepresentation);
        }

        public bool CanUndo()
        {
            return moves.Count > 0;
        }

        public bool CanRedo()
        {
            return true;
        }

        public void UndoLastMove()
        {
            
        }

        public void RedoLastMove()
        {

        }

        public bool HintsUsed()
        {
            return hintsUsed;
        }

        public bool IsFinished()
        {
            return puzzle.pieceCount == 1;
        }

        public void CleanUp()
        {
            foreach(Image i in images.Keys)
            {
                BoardGrid.Children.Remove(i);
            }
            currentImage = null;
            images.Clear();
            moves.Clear();
            puzzle = null;
            hintsUsed = false;
        }

        private void undoButton_Click(object sender, RoutedEventArgs e)
        {
            if(moves.Count > 0)
            {
                Move m = moves.Last();
                moves.Remove(m);
                int toCol = m.c2;
                int toRow = m.r2;
                char originalPiece = puzzle.board[toRow][toCol].piece;
                foreach(Image i in images.Keys)
                {
                    if(images[i].Key == toRow && images[i].Value == toCol && i.Visibility != Visibility.Collapsed)
                    {
                        images[i] = new KeyValuePair<int, int>(m.r1, m.c1);
                        Grid.SetRow(i, m.r1);
                        Grid.SetColumn(i, m.c1);
                        Image captured = capturedPieces.Last();
                        capturedPieces.Remove(captured);
                        captured.Visibility = Visibility.Visible;
                        break;
                    }
                }
                puzzle.board[m.r1][m.c1].piece = originalPiece;
                puzzle.board[toRow][toCol].piece = m.captured;
                puzzle.pieceCount++;
            }
        }

        private void RedoButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            SetPuzzle(originalPuzzleRepresentation);
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Grid_DragOver(object sender, DragEventArgs e)
        {
            currentImage.Visibility = Visibility.Collapsed;
            GridDragOverCustomized(sender, e);
        }

        private async void Grid_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();
                if (items.Count > 0)
                {

                }
            }
        }

        private void BoardGrid_DragEnter(object sender, DragEventArgs e)
        {
            currentImage.Visibility = Visibility.Collapsed;
            GridDragOverCustomized(sender, e);
        }

        private void Grid_DragOver_1(object sender, DragEventArgs e)
        {
            currentImage.Visibility = Visibility.Collapsed;
            GridDragOverCustomized(sender, e);
        }

        private void Page_DragOver(object sender, DragEventArgs e)
        {
            currentImage.Visibility = Visibility.Collapsed;
            GridDragOverCustomized(sender, e);
        }

        private void a4_Drop(object sender, DragEventArgs e)
        {
            Move m = new Move();
            int row = images[currentImage].Key;
            int col = images[currentImage].Value;
            m = m.DoMove(puzzle, row, col, 0, 0);
            if(m != null)
            {
                foreach (Image i in images.Keys)
                {
                    if (images[i].Key == 0 && images[i].Value == 0 && i.Visibility == Visibility.Visible)
                    {
                        i.Visibility = Visibility.Collapsed;
                        capturedPieces.Add(i);
                        Grid.SetRow(currentImage, 0);
                        Grid.SetColumn(currentImage, 0);
                        images[currentImage] = new KeyValuePair<int, int>(0, 0);
                        break;
                    }
                }
                moves.Add(m);
                puzzle.pieceCount--;
            }
        }

        private void b4_Drop(object sender, DragEventArgs e)
        {
            Move m = new Move();
            int row = images[currentImage].Key;
            int col = images[currentImage].Value;
            m = m.DoMove(puzzle, row, col, 0, 1);
            if (m != null)
            {
                foreach(Image i in images.Keys)
                {
                    if(images[i].Key == 0 && images[i].Value == 1 && i.Visibility == Visibility.Visible)
                    {
                        i.Visibility = Visibility.Collapsed;
                        capturedPieces.Add(i);
                        Grid.SetRow(currentImage, 0);
                        Grid.SetColumn(currentImage, 1);
                        images[currentImage] = new KeyValuePair<int, int>(0, 1);
                        break;
                    }
                }
                moves.Add(m);
                puzzle.pieceCount--;
            }
        }

        private void c4_Drop(object sender, DragEventArgs e)
        {
            Move m = new Move();
            int row = images[currentImage].Key;
            int col = images[currentImage].Value;
            m = m.DoMove(puzzle, row, col, 0, 2);
            if (m != null)
            {
                foreach (Image i in images.Keys)
                {
                    if (images[i].Key == 0 && images[i].Value == 2 && i.Visibility == Visibility.Visible)
                    {
                        i.Visibility = Visibility.Collapsed;
                        capturedPieces.Add(i);
                        Grid.SetRow(currentImage, 0);
                        Grid.SetColumn(currentImage, 2);
                        images[currentImage] = new KeyValuePair<int, int>(0, 2);
                        break;
                    }
                }
                moves.Add(m);
                puzzle.pieceCount--;
            }
        }

        private void d4_Drop(object sender, DragEventArgs e)
        {
            Move m = new Move();
            int row = images[currentImage].Key;
            int col = images[currentImage].Value;
            m = m.DoMove(puzzle, row, col, 0, 3);
            if (m != null)
            {
                foreach (Image i in images.Keys)
                {
                    if (images[i].Key == 0 && images[i].Value == 3 && i.Visibility == Visibility.Visible)
                    {
                        i.Visibility = Visibility.Collapsed;
                        capturedPieces.Add(i);
                        Grid.SetRow(currentImage, 0);
                        Grid.SetColumn(currentImage, 3);
                        images[currentImage] = new KeyValuePair<int, int>(0, 3);
                        break;
                    }
                }
                moves.Add(m);
                puzzle.pieceCount--;
            }
        }

        private void a3_Drop(object sender, DragEventArgs e)
        {
            Move m = new Move();
            int row = images[currentImage].Key;
            int col = images[currentImage].Value;
            m = m.DoMove(puzzle, row, col, 1, 0);
            if (m != null)
            {
                foreach (Image i in images.Keys)
                {
                    if (images[i].Key == 1 && images[i].Value == 0 && i.Visibility == Visibility.Visible)
                    {
                        i.Visibility = Visibility.Collapsed;
                        capturedPieces.Add(i);
                        Grid.SetRow(currentImage, 1);
                        Grid.SetColumn(currentImage, 0);
                        images[currentImage] = new KeyValuePair<int, int>(1, 0);
                        break;
                    }
                }
                moves.Add(m);
                puzzle.pieceCount--;
            }
        }

        private void b3_Drop(object sender, DragEventArgs e)
        {
            Move m = new Move();
            int row = images[currentImage].Key;
            int col = images[currentImage].Value;
            m = m.DoMove(puzzle, row, col, 1, 1);
            if (m != null)
            {
                foreach (Image i in images.Keys)
                {
                    if (images[i].Key == 1 && images[i].Value == 1 && i.Visibility == Visibility.Visible)
                    {
                        i.Visibility = Visibility.Collapsed;
                        capturedPieces.Add(i);
                        Grid.SetRow(currentImage, 1);
                        Grid.SetColumn(currentImage, 1);
                        images[currentImage] = new KeyValuePair<int, int>(1, 1);
                        break;
                    }
                }
                moves.Add(m);
                puzzle.pieceCount--;
            }
        }

        private void c3_Drop(object sender, DragEventArgs e)
        {
            Move m = new Move();
            int row = images[currentImage].Key;
            int col = images[currentImage].Value;
            m = m.DoMove(puzzle, row, col, 1, 2);
            if (m != null)
            {
                foreach (Image i in images.Keys)
                {
                    if (images[i].Key == 1 && images[i].Value == 2 && i.Visibility == Visibility.Visible)
                    {
                        i.Visibility = Visibility.Collapsed;
                        capturedPieces.Add(i);
                        Grid.SetRow(currentImage, 1);
                        Grid.SetColumn(currentImage, 2);
                        images[currentImage] = new KeyValuePair<int, int>(1, 2);
                        break;
                    }
                }
                moves.Add(m);
                puzzle.pieceCount--;
            }
        }

        private void d3_Drop(object sender, DragEventArgs e)
        {
            Move m = new Move();
            int row = images[currentImage].Key;
            int col = images[currentImage].Value;
            m = m.DoMove(puzzle, row, col, 1, 3);
            if (m != null)
            {
                foreach (Image i in images.Keys)
                {
                    if (images[i].Key == 1 && images[i].Value == 3 && i.Visibility == Visibility.Visible)
                    {
                        i.Visibility = Visibility.Collapsed;
                        capturedPieces.Add(i);
                        Grid.SetRow(currentImage, 1);
                        Grid.SetColumn(currentImage, 3);
                        images[currentImage] = new KeyValuePair<int, int>(1, 3);
                        break;
                    }
                }
                moves.Add(m);
                puzzle.pieceCount--;
            }
        }

        private void a2_Drop(object sender, DragEventArgs e)
        {
            Move m = new Move();
            int row = images[currentImage].Key;
            int col = images[currentImage].Value;
            m = m.DoMove(puzzle, row, col, 2, 0);
            if (m != null)
            {
                foreach (Image i in images.Keys)
                {
                    if (images[i].Key == 2 && images[i].Value == 0 && i.Visibility == Visibility.Visible)
                    {
                        i.Visibility = Visibility.Collapsed;
                        capturedPieces.Add(i);
                        Grid.SetRow(currentImage, 2);
                        Grid.SetColumn(currentImage, 0);
                        images[currentImage] = new KeyValuePair<int, int>(2, 0);
                        break;
                    }
                }
                moves.Add(m);
                puzzle.pieceCount--;
            }
        }

        private void b2_Drop(object sender, DragEventArgs e)
        {
            Move m = new Move();
            int row = images[currentImage].Key;
            int col = images[currentImage].Value;
            m = m.DoMove(puzzle, row, col, 2, 1);
            if (m != null)
            {
                foreach (Image i in images.Keys)
                {
                    if (images[i].Key == 2 && images[i].Value == 1 && i.Visibility == Visibility.Visible)
                    {
                        i.Visibility = Visibility.Collapsed;
                        capturedPieces.Add(i);
                        Grid.SetRow(currentImage, 2);
                        Grid.SetColumn(currentImage, 1);
                        images[currentImage] = new KeyValuePair<int, int>(2, 1);
                        break;
                    }
                }
                moves.Add(m);
                puzzle.pieceCount--;
            }
        }

        private void c2_Drop(object sender, DragEventArgs e)
        {
            Move m = new Move();
            int row = images[currentImage].Key;
            int col = images[currentImage].Value;
            m = m.DoMove(puzzle, row, col, 2, 2);
            if (m != null)
            {
                foreach (Image i in images.Keys)
                {
                    if (images[i].Key == 2 && images[i].Value == 2 && i.Visibility == Visibility.Visible)
                    {
                        i.Visibility = Visibility.Collapsed;
                        capturedPieces.Add(i);
                        Grid.SetRow(currentImage, 2);
                        Grid.SetColumn(currentImage, 2);
                        images[currentImage] = new KeyValuePair<int, int>(2, 2);
                        break;
                    }
                }
                moves.Add(m);
                puzzle.pieceCount--;
            }
        }

        private void d2_Drop(object sender, DragEventArgs e)
        {
            Move m = new Move();
            int row = images[currentImage].Key;
            int col = images[currentImage].Value;
            m = m.DoMove(puzzle, row, col, 2, 3);
            if (m != null)
            {
                foreach (Image i in images.Keys)
                {
                    if (images[i].Key == 2 && images[i].Value == 3 && i.Visibility == Visibility.Visible)
                    {
                        i.Visibility = Visibility.Collapsed;
                        capturedPieces.Add(i);
                        Grid.SetRow(currentImage, 2);
                        Grid.SetColumn(currentImage, 3);
                        images[currentImage] = new KeyValuePair<int, int>(2, 3);
                        break;
                    }
                }
                moves.Add(m);
                puzzle.pieceCount--;
            }
        }

        private void a1_Drop(object sender, DragEventArgs e)
        {
            Move m = new Move();
            int row = images[currentImage].Key;
            int col = images[currentImage].Value;
            m = m.DoMove(puzzle, row, col, 3, 0);
            if (m != null)
            {
                foreach (Image i in images.Keys)
                {
                    if (images[i].Key == 3 && images[i].Value == 0 && i.Visibility == Visibility.Visible)
                    {
                        i.Visibility = Visibility.Collapsed;
                        capturedPieces.Add(i);
                        Grid.SetRow(currentImage, 3);
                        Grid.SetColumn(currentImage, 0);
                        images[currentImage] = new KeyValuePair<int, int>(3, 0);
                        break;
                    }
                }
                moves.Add(m);
                puzzle.pieceCount--;
            }
        }

        private void b1_Drop(object sender, DragEventArgs e)
        {
            Move m = new Move();
            int row = images[currentImage].Key;
            int col = images[currentImage].Value;
            m = m.DoMove(puzzle, row, col, 3, 1);
            if (m != null)
            {
                foreach (Image i in images.Keys)
                {
                    if (images[i].Key == 3 && images[i].Value == 1 && i.Visibility == Visibility.Visible)
                    {
                        i.Visibility = Visibility.Collapsed;
                        capturedPieces.Add(i);
                        Grid.SetRow(currentImage, 3);
                        Grid.SetColumn(currentImage, 1);
                        images[currentImage] = new KeyValuePair<int, int>(3, 1);
                        break;
                    }
                }
                moves.Add(m);
                puzzle.pieceCount--;
            }
        }

        private void c1_Drop(object sender, DragEventArgs e)
        {
            Move m = new Move();
            int row = images[currentImage].Key;
            int col = images[currentImage].Value;
            m = m.DoMove(puzzle, row, col, 3, 2);
            if (m != null)
            {
                foreach (Image i in images.Keys)
                {
                    if (images[i].Key == 3 && images[i].Value == 2 && i.Visibility == Visibility.Visible)
                    {
                        i.Visibility = Visibility.Collapsed;
                        capturedPieces.Add(i);
                        Grid.SetRow(currentImage, 3);
                        Grid.SetColumn(currentImage, 2);
                        images[currentImage] = new KeyValuePair<int, int>(3, 2);
                        break;
                    }
                }
                moves.Add(m);
                puzzle.pieceCount--;
            }
        }

        private void d1_Drop(object sender, DragEventArgs e)
        {
            Move m = new Move();
            int row = images[currentImage].Key;
            int col = images[currentImage].Value;
            m = m.DoMove(puzzle, row, col, 3, 3);
            if (m != null)
            {
                foreach (Image i in images.Keys)
                {
                    if (images[i].Key == 3 && images[i].Value == 3 && i.Visibility == Visibility.Visible)
                    {
                        i.Visibility = Visibility.Collapsed;
                        capturedPieces.Add(i);
                        Grid.SetRow(currentImage, 3);
                        Grid.SetColumn(currentImage, 3);
                        images[currentImage] = new KeyValuePair<int, int>(3, 3);
                        break;
                    }
                }
                moves.Add(m);
                puzzle.pieceCount--;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            CleanUp();
            var frame = (Frame)Window.Current.Content;
            frame.Navigate(typeof(Menu));
        }

        private void HintButton_Click(object sender, RoutedEventArgs e)
        {
            ShowHint();
        }
    }
}
