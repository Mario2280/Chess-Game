using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System;
using System.Runtime.CompilerServices;

namespace ChessLogics
{
    public enum Figures
    {
        none,

        whiteKing = 'K',
        whiteQueen = 'Q',
        whiteRook = 'R',
        whiteBishop = 'B',
        whiteKnight = 'N',
        whitePawn = 'P',

        blackKing = 'k',
        blackQueen = 'q',
        blackRook = 'r',
        blackBishop = 'b',
        blackKnight = 'n',
        blackPawn = 'p'

    }
    public class Chess : MonoBehaviour
    {

        public string fen;

        public Board board { get; }

        Moves moves;

        FigureMoving prev;

        static string temp = "ZZ9Z9";//строка для хранения предыдущего хода

        public static string Promotion;

        List<FigureMoving> allMoves;

        public bool FlagPromotion;

        public bool Correct;




        //2 конструктора 1)по fen 2)по Board для Chess.Move
        public Chess(string fen = "rnbqkbnr/pppppppp/8/4N3/8/5Q2/PPPPPPPP/RNB1KB1R w KQkq - 0 1", bool ChangeSide = false)//"r2qk2r/pppppppp/8/8/8/8/PPPPPPPP/R3K2R w KQkq - 0 1"
        {
            this.fen = fen;
            this.board = new Board(fen);
            if (ChangeSide)
            {
                this.board.moveNumber++;
                this.board.moveColor = (this.board.moveNumber % 2 == 0 ? Color.black : Color.white);
                
            }
            //Moves.Get();
            moves = new Moves(board);
        }


        Chess(Board board)
        {
            this.board = board;
            this.fen = board.fen;
            moves = new Moves(board);

        }


        //метод совершения хода 
        public Chess Move(string move, int BtnPromotion, int FlagPrev)
        {
            FigureMoving fm = new FigureMoving(move);
            FlagPromotion = false;
            Correct = false;
            prev = new FigureMoving(temp);
            if (prev == new FigureMoving("Pa1a1"))
                moves.Getflag(prev.Check(prev, fm));
            if (!moves.CanMove(fm) || board.isCheckAfterMove(fm))
                return this;
            Correct = true;
            if ((fm.figure == Figures.whiteKing || fm.figure == Figures.blackKing) && fm.AbsDx != 2)
            {
                if (fm.figure == Figures.whiteKing)
                {
                    Moves.CastWhiteLeft = false;
                    Moves.CastWhiteRight = false;
                }
                else
                {
                    Moves.CastBlackLeft = false;
                    Moves.CastBlackRight = false;
                }
            }
            if (fm.figure == Figures.blackRook || fm.figure == Figures.whiteRook)
            {
                if (fm.figure == Figures.whiteRook)
                {
                    if (fm.from == new Square("a1"))
                        Moves.CastWhiteLeft = false;
                    if (fm.from == new Square("h1"))
                        Moves.CastWhiteRight = false;
                }
                if (fm.from == new Square("a8"))
                    Moves.CastBlackLeft = false;
                if (fm.from == new Square("h8"))
                    Moves.CastBlackRight = false;
            }
            if((fm.figure == Figures.blackPawn && fm.from.y == 1 && fm.to.y == 0) || (fm.figure == Figures.whitePawn && fm.from.y == 6 && fm.to.y == 7))
            {
                FlagPromotion = true;
                Color temp = (fm.figure.GetColor() == Color.white ? Color.white : Color.black);
                switch (BtnPromotion)
                {
                    case 1:
                        fm.figure = (temp == Color.white ? Figures.whiteQueen : Figures.blackQueen);
                        break;
                    case 2:
                        fm.figure = (temp == Color.white ? Figures.whiteBishop : Figures.blackBishop);
                        break;
                    case 3:
                        fm.figure = (temp == Color.white ? Figures.whiteRook : Figures.blackRook);
                        break;
                    case 4:
                        fm.figure = (temp == Color.white ? Figures.whiteKnight : Figures.blackKnight);
                        break;
                    default:
                        break;


                }  
            }
            if (FlagPrev == 1)
                temp = move;
            board.Getflag(prev.Check(prev, fm));
            Board nextBoard = board.Move(fm);
            Chess nextChess = new Chess(nextBoard);
            Promotion = "";
            return nextChess;
        }


        //Метод возвращает буквенное представление фигуры
        public char GetFigureAt(int x, int y)
        {
            Square square = new Square(x, y);
            Figures f = board.GetFigureAt(square);
            return f == Figures.none ? '.' : (char)f;
        }

        void FindAllMoves()
        {
            allMoves = new List<FigureMoving>();
            foreach (FigureOnSquare fs in board.YieldFigures())//перебираю все фигуры на доске, того цвета,которые сейчас ходят
                foreach (Square to in Square.YieldSquares())//перебираю все клетки на доске куда можем пойти(на их пересечении всевозможные ходы)
                {
                    FigureMoving fm = new FigureMoving(fs, to);//куда-то идущая фигур
                    prev = new FigureMoving(temp);
                    board.Getflag(prev.Check(prev, fm));
                    moves.Getflag(prev.Check(prev, fm));
                    //if (moves.CanMove(fm))//если этот ход возможен добавляем его в список всевозм.ходов
                    if (!(!moves.CanMove(fm) || board.isCheckAfterMove(fm)))
                            allMoves.Add(fm);
                    //if (!board.isCheckAfterMove(fm))


                }

        }

        //Методы проверки на шах мат и пат
        public bool IsCheck() { return board.isCheck(); }

        public bool isCheckmate() { return (IsCheck() && GetAllMoves().Count == 0); }

        public bool isStalemate() { return (!(IsCheck()) && GetAllMoves().Count == 0); }

        public List<string> GetAllMoves()
        {
            FindAllMoves();
            List<string> list = new List<string>();
            foreach (FigureMoving fm in allMoves)
                list.Add(fm.ToString());
            return list;
        }

        public List<string> GetAllMovesSelectedFigure(string move)
        {
            FindAllMoves();
            List<string> list = new List<string>();
            foreach (FigureMoving fm in allMoves)
            {
                if(fm.ToString().Substring(0,3) == move)
                    list.Add(fm.ToString());
            }   
            return list;
        }

    }
    public class Board : MonoBehaviour
    {
        public string fen { get; private set; }
        public Figures[,] figures { get; set; }
        public Color moveColor { set; get; }
        public int moveNumber { get; set; }
        public Board(string fen)
        {
            this.fen = fen;
            figures = new Figures[8, 8];
            Init();
        }

        public bool flag;

        //инициализация доски по fen
        //rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1
        //0                                           1 2    3 4 5
        //0 расположение фигур
        //            //1 чей ход
        //            //2 условие ракировка(флаги ракировки
        //            //3 битое поле 
        //            //4 кол-во ходов(правило 50 ходов)
        //            //5 № хода сейчас  
        //            //234 опустил
        void Init()
        {
            string[] parts = fen.Split();
            if (parts.Length != 6) return;
            InitFigures(parts[0]);
            moveColor = (parts[1] == "b" ? Color.black : Color.white);
            moveNumber = int.Parse(parts[5]);
            moveColor = Color.white;
        }

        public void Getflag(bool flag)
        {
            this.flag = flag;
        }


        //Растановка фигур по кусочку fen
        void InitFigures(string data)
        {
            for (int j = 8; j >= 2; j--)
                data = data.Replace(j.ToString(), (j - 1).ToString() + "1");
            data = data.Replace("1", ".");
            string[] lines = data.Split('/');
            for (int y = 7; y >= 0; y--)
            {
                for (int x = 0; x < 8; x++)
                {
                    figures[x, y] = ((lines[7 - y][x] == '.') ? Figures.none : (Figures)lines[7 - y][x]);
                }
            }

        }


        //Возврат фигуры по клетке
        public Figures GetFigureAt(Square square)
        {
            if (square.OnBoard())
                return figures[square.x, square.y];
            return Figures.none;
        }


        //Внутренний метод для установки фигуры в массив(после создания объекта не применяется,но во время создания применяется)
        void SetFigureAt(Square square, Figures figure)
        {
            if (square.OnBoard())
                figures[square.x, square.y] = figure;
        }

        //перерисовка доски для нового хода
        public Board Move(FigureMoving fm)
        {
            //fm.GetPromotion(Chess.Promotion);
            Board next = new Board(fen);
            Moves temp = new Moves(next);
            if (flag)
            {
                Square remove = new Square(/*fm.from.x + fm.Dx*/fm.to.x, fm.to.y - 1 * fm.SignY);
                next.SetFigureAt(fm.from, Figures.none);
                next.SetFigureAt(remove, Figures.none);
                next.SetFigureAt(fm.to, fm.promotion == Figures.none ? fm.figure : fm.promotion);
                flag = false;
            }
            next.SetFigureAt(fm.from, Figures.none);
            next.SetFigureAt(fm.to, fm.promotion == Figures.none ? fm.figure : fm.promotion);
            if ((fm.figure == Figures.blackKing || fm.figure == Figures.whiteKing) && fm.AbsDx == 2 && fm.AbsDy == 0)
            {
                if (fm.figure.GetColor() == Color.white)
                {

                    if (fm.Dx > 0)
                    {
                        next.SetFigureAt(new Square("h1"), Figures.none);
                        next.SetFigureAt(new Square("f1"), Figures.whiteRook);
                    }
                    else
                    {
                        next.SetFigureAt(new Square("a1"), Figures.none);
                        next.SetFigureAt(new Square("d1"), Figures.whiteRook);
                    }
                }

                else
                {
                    if (fm.Dx > 0)
                    {
                        next.SetFigureAt(new Square("h8"), Figures.none);
                        next.SetFigureAt(new Square("f8"), Figures.blackRook);
                    }
                    else
                    {
                        next.SetFigureAt(new Square("a8"), Figures.none);
                        next.SetFigureAt(new Square("d8"), Figures.blackRook);
                    }
                }
            }
            //if (moveColor == Color.black)
            next.moveNumber++;
            next.moveColor = moveColor.FlipColor();
            next.GenerateFen();//функция для сохранения ходов чтобы менять позицию
            return next;
        }

        //внутренний метод создания нового fen для нового хода
        void GenerateFen()
        {
            fen = FenFigures() + " " +
                    (moveColor == Color.white ? "w" : "b")
                    + " - - 0 " + moveNumber.ToString();
        }


        //внутренний метод для создания fen по массиву фигур(позиция на доске)    
        string FenFigures()
        {
            StringBuilder sb = new StringBuilder();
            for (int y = 7; y >= 0; y--)
            {
                for (int x = 0; x < 8; x++)
                {
                    sb.Append(figures[x, y] == Figures.none ? '1' : (char)figures[x, y]);
                }
                if (y > 0) sb.Append('/');
            }
            string eight = "11111111";
            for (int j = 8; j >= 2; j--)
                sb.Replace(eight.Substring(0, j), j.ToString());
            return sb.ToString();
        }


        //Выборка всех фигур цвета тек.хода
        public IEnumerable<FigureOnSquare> YieldFigures()
        {
            foreach (Square square in Square.YieldSquares())//перебо всех квадратов
                if (GetFigureAt(square).GetColor() == moveColor)//на текущем квадрате фигура цвета текущего хода и цвета--> мы её возвращаем
                    yield return new FigureOnSquare(GetFigureAt(square), square);
        }

        public bool isCheck()//проверка на шах в текущий момент без проверки шаха после хода
        {
            Board after = new Board(fen);
            after.moveColor = moveColor.FlipColor();
            return after.CanEatKing();
        }

        public bool isCheck(string fen)//проверка на шах в текущий момент без проверки шаха после хода
        {
            Board after = new Board(fen);
            after.moveColor = moveColor.FlipColor();
            return after.CanEatKing();
        }

        public bool CanEatKing()
        {
            Square badKing = FindBadKing();
            Moves moves = new Moves(this);
            foreach (FigureOnSquare fs in YieldFigures())
            {
                FigureMoving fm = new FigureMoving(fs, badKing);
                if (moves.CanMove(fm))
                    return true;
            }
            return false;
        }

        public Square FindBadKing()
        {
            Figures badKing = moveColor == Color.black ? Figures.whiteKing : Figures.blackKing;

            foreach (Square square in Square.YieldSquares())
                if (GetFigureAt(square) == badKing)
                    return square;
            return Square.none;
        }

        public bool isCheckAfterMove(FigureMoving fm)//проверка на шах после хода 
        {
            Board after = Move(fm);
            after.moveColor = moveColor.FlipColor();
            return after.CanEatKing();
        }

        public bool isCheckAfter2Move(FigureMoving fm1, FigureMoving fm2)//проверка на шах после хода 
        {
            Board after = Move(fm1);
            after.moveColor = moveColor.FlipColor();
            if (after.CanEatKing())
                return false;
            after = Move(fm2);
            if (after.CanEatKing())
                return false;
            return true;

        }

    }
    public class FigureMoving : MonoBehaviour
    {
        public static FigureMoving none = new FigureMoving("ZZ9Z9");
        //Свойства 1)фигура (2-3)A->B 3)превращение(в ферзя пешки)
        public Figures figure { get;  set; }
        public Square from { get; private set; }
        public Square to { get; private set; }
        public Figures promotion;
        //Cвойства флаги для рокировки
        static bool CastBlackFlag { get; set; }
        static bool CastFlagWhite { get; set; }


        //3 конструктора 1) для числовых координат 2)для парсинга строки Pe2e4
        public FigureMoving(FigureOnSquare fs, Square to, Figures promotion = Figures.none)
        {
            this.figure = fs.figure;
            this.from = fs.square;
            this.to = to;
            CastBlackFlag = true;
            CastFlagWhite = true;
        }
        public FigureMoving(string move)
        {
            //Pe7e8Q
            //012345
            this.figure = (Figures)move[0];
            this.from = new Square(move.Substring(1, 2));
            this.to = new Square(move.Substring(3, 2));
            this.promotion = (move.Length == 6) ? (Figures)move[5] : Figures.none;
            CastBlackFlag = true;
            CastFlagWhite = true;
        }

        public void GetCastleFlag(bool flag, Color color)
        {
            switch (color)
            {
                case Color.white:
                    CastFlagWhite = flag;
                    break;
                case Color.black:
                    CastBlackFlag = flag;
                    break;
            }
        }

        public FigureMoving(Figures fs, Square from, Square to, Figures promotion = Figures.none)
        {
            figure = fs;
            this.from = from;
            this.to = to;
            this.promotion = promotion;
            CastBlackFlag = true;
            CastFlagWhite = true;
        }

        public int Dx { get { return to.x - from.x; } }
        public int Dy { get { return to.y - from.y; } }

        public int AbsDx { get { return Math.Abs(Dx); } }
        public int AbsDy { get { return Math.Abs(Dy); } }

        public int SignX { get { return Math.Sign(Dx); } }
        public int SignY { get { return Math.Sign(Dy); } }

        public override string ToString()
        {
            string text = (char)figure + from.Name + to.Name;
            if (promotion != Figures.none)
                text += (char)promotion;
            return text;
        }

        public bool Check(FigureMoving prev, FigureMoving now)
        {
            int stepY = prev.figure.GetColor() == Color.white ? 1 : -1;//направление движения в зависимости от цвета
            if (prev.Dx == 0)//если идёт прямо
                if (prev.Dy == 2 * stepY)
                    if (now.from.y == 3 || now.from.y == 4)
                        if ((prev.from.x == now.from.x + 1 || prev.from.x == now.from.x - 1) && now.to.x == prev.to.x)
                            if (now.to.y == prev.to.y - 1)
                                return true;
            return false;
        }

        public bool Castle(FigureMoving now, Board board)
        {
            //если король уже ходил
            if ((CastBlackFlag == false && board.moveColor == Color.black) ||
                (CastFlagWhite == false && board.moveColor == Color.white))
                return false;
            if (now.figure == Figures.blackKing || now.figure == Figures.whiteKing)
                if (now.AbsDx == 2 && now.AbsDy == 0)
                {
                    for (int i = 0; i <= 1; i++)
                    {
                        Square tempto = new Square(now.from.x + now.SignX * (i + 1), now.from.y);
                        Square tempfrom = new Square(now.from.x + now.SignX * i, now.from.y);
                        FigureMoving next = new FigureMoving(now.figure, tempfrom, tempto, now.promotion);
                        Moves moves = new Moves(board);
                        if (!moves.CanMove(next) || board.isCheckAfterMove(next))
                            return false;
                    }
                    
                    switch (now.figure.GetColor())
                    {
                        case Color.white:
                            if (Figures.whiteKing == board.GetFigureAt(new Square("e1")))
                                if (now.Dx > 0 && Figures.whiteRook == board.GetFigureAt(new Square("h1")) ||
                                    now.Dx < 0 && Figures.whiteRook == board.GetFigureAt(new Square("a1")))
                                    return true;
                            break;
                        case Color.black:
                            if (Figures.blackKing == board.GetFigureAt(new Square("e8")))
                                if (now.Dx > 0 && Figures.blackRook == board.GetFigureAt(new Square("h8")) ||
                                    now.Dx < 0 && Figures.blackRook == board.GetFigureAt(new Square("a8")))
                                    return true;
                            break;
                    }

                }
            return false;
        }
    }
    public class Moves : MonoBehaviour
    {
        FigureMoving fm;

        Board board;

        bool flag;//взятие на проходе


        public static bool CastBlackLeft = true, CastBlackRight = true, CastWhiteLeft = true, CastWhiteRight = true;

        public void Getflag(bool flag)
        {
            this.flag = flag;
        }

        public static void Get()
        {
            CastBlackLeft = true; CastBlackRight = true; CastWhiteLeft = true; CastWhiteRight = true;
        }


        public Moves(Board board)
        {
            this.board = board;
            this.flag = false;
        }

        public bool CanMove(FigureMoving fm)
        {
            this.fm = fm;

            return (
                CanMoveFrom() &&
                CanMoveTo() &&
                CanFigureMove());//Проверка если после хода открывается шах и может ли фигура в принципе так ходить
        }

        bool CanMoveFrom()
        {
            return fm.from.OnBoard() &&
                   fm.figure.GetColor() == board.moveColor &&
                    board.GetFigureAt(fm.from) == fm.figure;

        }

        bool CanMoveTo()
        {
            return fm.to.OnBoard() &&
                fm.from != fm.to &&
                board.GetFigureAt(fm.to).GetColor() != board.moveColor;//не бей своих чтобы чужие боялись
        }

        bool CanFigureMove()
        {

            switch (fm.figure)
            {
                case Figures.whiteKing:
                case Figures.blackKing:
                    return CanKingMove() || (CanCastle() && !board.isCheck());

                case Figures.whiteQueen:
                case Figures.blackQueen:
                    return CanStraightMove();//движение вперёд во всех направленях (для ферзя)

                case Figures.whiteRook:
                case Figures.blackRook:
                    return (fm.SignX == 0 || fm.SignY == 0) &&
                        CanStraightMove();

                case Figures.whiteBishop:
                case Figures.blackBishop:
                    return (fm.SignX != 0 && fm.SignY != 0) &&
                       CanStraightMove();

                case Figures.whitePawn:
                case Figures.blackPawn:
                    return CanPawnMove();

                case Figures.whiteKnight:
                case Figures.blackKnight:
                    return CanKnightMove();

                default: return false;
            }
        }


        private bool CanKingMove()
        {
            if (fm.AbsDx <= 1 && fm.AbsDy <= 1)
            {
                return true;
            }
            return false;
        }

        private bool CanCastle()
        {

            string temp_fen = board.fen;
            Board after = new Board(temp_fen);
            if (fm.AbsDx == 2 && fm.AbsDy == 0)
                switch (fm.figure.GetColor())
                {
                    case Color.white:
                        {
                            if ((CastWhiteLeft == false && fm.Dx < 0) || (CastWhiteRight == false && fm.Dx > 0))
                                return false;

                            Square tempto1 = new Square(fm.from.x + fm.SignX, fm.from.y);
                            Square tempfrom1 = new Square(fm.from.x, fm.from.y);
                            Square tempto2 = new Square(fm.from.x + fm.SignX * 2, fm.from.y);
                            Square tempfrom2 = new Square(fm.from.x + fm.SignX, fm.from.y);
                            FigureMoving part1 = new FigureMoving(fm.figure, tempfrom1, tempto1, fm.promotion);
                            FigureMoving part2 = new FigureMoving(fm.figure, tempfrom2, tempto2, fm.promotion);
                            after.moveColor = after.moveColor.FlipColor();

                            if (!after.isCheckAfter2Move(part1, part2))
                                return false;


                            return true;
                        }


                    case Color.black:
                        {
                            if ((CastBlackLeft == false && fm.Dx < 0) || (CastBlackRight == false && fm.Dx > 0))
                                return false;
                            Square tempto3 = new Square(fm.from.x + fm.SignX, fm.from.y);
                            Square tempfrom3 = new Square(fm.from.x, fm.from.y);
                            Square tempto4 = new Square(fm.from.x + fm.SignX * 2, fm.from.y);
                            Square tempfrom4 = new Square(fm.from.x + fm.SignX, fm.from.y);
                            FigureMoving part3 = new FigureMoving(fm.figure, tempfrom3, tempto3, fm.promotion);
                            FigureMoving part4 = new FigureMoving(fm.figure, tempfrom4, tempto4, fm.promotion);
                            after.moveColor = after.moveColor.FlipColor();

                            if (!after.isCheckAfter2Move(part3, part4))
                                return false;

                            return true;
                        }
                }
            return false;

        }

        private bool CanKnightMove()
        {
            if (fm.AbsDx == 1 && fm.AbsDy == 2) return true;
            if (fm.AbsDx == 2 && fm.AbsDy == 1) return true;
            return false;
        }

        private bool CanStraightMove()
        {
            Square at = fm.from;//квадрат, откуда мы идём
            do
            {
                at = new Square(at.x + fm.SignX, at.y + fm.SignY);//новый квадрат со смещением хотя бы на 1 клетку
                if (at == fm.to)
                    return true;//пока не прийдём в пункт назначения 
            } while (at.OnBoard() &&
                     board.GetFigureAt(at) == Figures.none);//пока не вышли за доску или не встретили фигуру на пути
            return false;
        }

        private bool CanPawnMove()
        {
            if (fm.from.y < 1 || fm.from.y > 6)
                return false;
            int stepY = fm.figure.GetColor() == Color.white ? 1 : -1;//направление движения в зависимости от цвета
            return (CanPawnGo(stepY) ||
                   CanPawnJump(stepY) ||
                   CanPawnEat(stepY) ||
                   this.flag);

        }



        private bool CanPawnEat(int stepY)
        {
            if (board.GetFigureAt(fm.to) != Figures.none)//если не на пустую клетку
                if (fm.AbsDx == 1)//если бьёт должно быть смещение
                    if (fm.Dy == stepY)//нужное направление
                        return true;
            return false;
        }

        private bool CanPawnJump(int stepY)
        {
            if (board.GetFigureAt(fm.to) == Figures.none)//если клетка на которую идёт пуста
                if (fm.Dx == 0)//если идёт прямо
                    if (fm.Dy == 2 * stepY)
                        if (fm.from.y == 1 || fm.from.y == 6)//на 2 только на 1 и 6 линии
                            if (board.GetFigureAt(new Square(fm.from.x, fm.from.y + stepY)) == Figures.none)//если этому не припяттвует к.л.фигура
                                return true;
            return false;
        }

        private bool CanPawnGo(int stepY)
        {
            if (board.GetFigureAt(fm.to) == Figures.none)//если клетка на которую идёт пуста
                if (fm.Dx == 0)//если идёт прямо
                    if (fm.Dy == stepY)//если верное направление
                        return true;
            return false;

        }
    }

    public class Square : MonoBehaviour
    {
        public static Square none = new Square(-1, -1);
        public int x { get; private set; }
        public int y { get; private set; }
        //2 конструктора 1) для числовых координат 2)для  строки e2e4
        public Square(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public Square(string e2)
        {
            if (e2.Length == 2 &&
                e2[0] >= 'a' &&
                e2[0] <= 'h' &&
                e2[1] >= 1 &&
                e2[1] <= '8')
            {
                x = e2[0] - 'a';
                y = e2[1] - '1';
            }
            else
            {
                this.x = -1;
                this.y = -1;
            }
        }
        //Проверка корд.на правильность
        public bool OnBoard()
        {
            return x >= 0 && x < 8 &&
                y >= 0 && y < 8;
        }

        //операторы сравнения == != на случай если фигура ходит с е3 на е3
        public static bool operator == (Square a, Square b)
        {
            return a.x == b.x && a.y == b.y;
        }

        public static bool operator != (Square a, Square b)
        {
            return !(a == b);
        }
        //Метод для преобразования числовых координат массива фигур(варианты ходов) к виду e2e4
        public string Name { get { return ((char)('a' + x)).ToString() + (y + 1).ToString(); } }


        //Перебор всех клеток
        public static IEnumerable<Square> YieldSquares()
        {
            for (int y = 0; y < 8; y++)
                for (int x = 0; x < 8; x++)
                    yield return new Square(x, y);
        }
    }

    public static class FiguresMethods
    {
        public static Color GetColor(this Figures figure)
        {
            if (figure == Figures.none)
                return Color.none;
            return (figure == Figures.whiteKing ||
                    figure == Figures.whiteQueen ||
                    figure == Figures.whiteKnight ||
                    figure == Figures.whiteRook ||
                    figure == Figures.whiteBishop ||
                    figure == Figures.whitePawn) ? Color.white : Color.black;
        }

    }

    public enum Color
    {
        none,
        white,
        black
    }
    //Вспомогательный класс для метода смены цвета т.к.в enum нельзя создать метод
    static class ColorMethods
    {
        //Функция переключения цвета(к ней можно обращаться от любого цвета enum)
        public static Color FlipColor(this Color color)
        {
            switch (color)
            {
                case Color.white:
                    return Color.black;
                case Color.black:
                    return Color.white;
                default:
                    return Color.none;
            }

        }
    }

    public class FigureOnSquare
    {
        public Figures figure { get; private set; }

        public Square square { get; private set; }

        public FigureOnSquare(Figures figure, Square square)
        {
            this.figure = figure;
            this.square = square;
        }
    }
}

