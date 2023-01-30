using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Digger
{
    class Terrain : ICreature
    {
        public string GetImageFileName()
        {
            return "Terrain.png";
        }

        public CreatureCommand Act(int x, int y)
        {
            return new CreatureCommand { DeltaX = 0 };
        }

        public int GetDrawingPriority()
        {
            return 5;
        }

        public bool DeadInConflict(ICreature conflictedObject)
        {
            if (conflictedObject.GetImageFileName() == "Digger.png" ) { return true; }
            return false;
        }     
    }
    class Player : ICreature
    {
        static public int X = 0;
        static public int Y = 0;
        public string GetImageFileName()
        {
            return "Digger.png";
        }

        public CreatureCommand Act(int x, int y)
        {
            if (Game.KeyPressed == System.Windows.Forms.Keys.Up && y - 1 >= 0 && !(Game.Map[x , y-1] is Sack))
                return new CreatureCommand() { DeltaX = 0, DeltaY = -1, TransformTo = null };
            else if (Game.KeyPressed == System.Windows.Forms.Keys.Right && x + 1 < Game.MapWidth && !(Game.Map[x+1,y] is Sack))
                return new CreatureCommand() { DeltaX = 1, DeltaY = 0, TransformTo = null };
            else if (Game.KeyPressed == System.Windows.Forms.Keys.Down && y + 1 < Game.MapHeight && !(Game.Map[x, y + 1] is Sack))
                return new CreatureCommand() { DeltaX = 0, DeltaY = 1, TransformTo = null };
            else if (Game.KeyPressed == System.Windows.Forms.Keys.Left && x - 1 >= 0 && !(Game.Map[x - 1, y] is Sack))
                return new CreatureCommand() { DeltaX = -1, DeltaY = 0, TransformTo = null };
            else
                return new CreatureCommand() { DeltaX = 0, DeltaY = 0, TransformTo = null };
        }

        public int GetDrawingPriority()
        {
            return 0;
        }

        public bool DeadInConflict(ICreature conflictedObject)
        {
            if (conflictedObject.GetImageFileName() == "Sack.png") return true;
            if (conflictedObject.GetImageFileName() == "Monster.png") return true;
            return false;
        }
    }
    public class Sack : ICreature
    {
        public int counter;
        public string GetImageFileName()
        {
            return "Sack.png";
        }

        public CreatureCommand Act(int x, int y)
        {
            if (y < Game.MapHeight - 1)
            {
                var map = Game.Map[x, y + 1];
                if (map == null || (counter > 0 && (map.ToString() == "Digger.Player" || map.ToString() == "Digger.Monster")))
                {
                    counter++;
                    return new CreatureCommand() { DeltaX = 0, DeltaY = 1 };
                }
            }
            if (counter > 1)
            {
                counter = 0;
                return new CreatureCommand() { DeltaX = 0, DeltaY = 0, TransformTo = new Gold() };
            }
            counter = 0;
            return new CreatureCommand() { DeltaX = 0, DeltaY = 0 };
        }

        public int GetDrawingPriority()
        {
            return 3;
        }

        public bool DeadInConflict(ICreature conflictedObject)
        {
            if (conflictedObject.GetImageFileName() == "Digger.png")
            {
                return false;
            }
            return false;
        }
    }
    public class Gold : ICreature
    {
        public string GetImageFileName()
        {
            return "Gold.png";
        }

        public CreatureCommand Act(int x, int y)
        {
            return new CreatureCommand { DeltaX = 0, DeltaY = 0, TransformTo = null };
        }

        public int GetDrawingPriority()
        {
            return 4;
        }

        public bool DeadInConflict(ICreature conflictedObject)
        {
            if (conflictedObject.GetImageFileName() == "Digger.png")
            {
                Game.Scores += 10;
                return true;
            }
            if (conflictedObject.GetImageFileName() == "Monster.png") return true;
            return false;
        }
    }
    public class Monster : ICreature
    {
        public string GetImageFileName()
        {
            return "Monster.png";
        }

        public CreatureCommand Act(int x, int y)
        {
            var xPlayer = -1; //невозможно для живого диггера
            var yPlayer = -1;
            double currentDist = 0;
            double nextDist = 0;
            int rows = Game.Map.GetUpperBound(0)+1;
            int columns = Game.Map.Length / rows;
            for (int i = 0; i < rows; i++) // перебор всей карты в поисках диггера
            {
                for (int j = 0; j < columns; j++)
                {
                    if (Game.Map[i,j] is Player)
                    {
                        xPlayer = i;
                        yPlayer = j;
                    }
                }
            }

            if (xPlayer == -1) //Мертвый игрок
            {
                return new CreatureCommand { DeltaX = 0, DeltaY = 0, TransformTo = null };
            }

            if (GameExtension.GetCreature(x,y+1) is Gold || GameExtension.GetCreature(x, y + 1) is null || GameExtension.GetCreature(x,y+1) is Player) //Вниз проверка
            {
                currentDist = Mathtematic.Distation(x, y, xPlayer, yPlayer); // дистанция сейчас до игрока
                nextDist = Mathtematic.Distation(x,y+1,xPlayer, yPlayer); // дистанция до игрока в следующей клетке
                if (currentDist > nextDist) return new CreatureCommand { DeltaX = 0, DeltaY = 1, TransformTo = null };
            }

            if (GameExtension.GetCreature(x-1, y) is Gold || GameExtension.GetCreature(x-1,y) is null || GameExtension.GetCreature(x-1, y) is Player) //Влево проверка
            {
                currentDist = Mathtematic.Distation(x, y, xPlayer, yPlayer);
                nextDist = Mathtematic.Distation(x-1, y, xPlayer, yPlayer);
                if (currentDist > nextDist) return new CreatureCommand { DeltaX = -1, DeltaY = 0, TransformTo = null };
            }

            if (GameExtension.GetCreature(x, y-1) is Gold || GameExtension.GetCreature(x, y-1) is null || GameExtension.GetCreature(x, y-1) is Player) //Вверх проверка
            {
                currentDist = Mathtematic.Distation(x, y, xPlayer, yPlayer);
                nextDist = Mathtematic.Distation(x, y-1, xPlayer, yPlayer);
                if (currentDist > nextDist) return new CreatureCommand { DeltaX = 0, DeltaY = -1, TransformTo = null };
            }

            if (GameExtension.GetCreature(x+1, y) is Gold || GameExtension.GetCreature(x+1, y) is null || GameExtension.GetCreature(x+1, y) is Player) //Вправо проверка
            {
                currentDist = Mathtematic.Distation(x, y, xPlayer, yPlayer);
                nextDist = Mathtematic.Distation(x+1, y, xPlayer, yPlayer);
                if (currentDist > nextDist) return new CreatureCommand { DeltaX = 1, DeltaY =0, TransformTo = null };
            }




            // Почти полное дублирование кода, вообщем если монстры застряли в стене при вычисление краткого пути а стен нет, то пусть попытаються выбраться. и найти короткую дистанцию
            // я хз Насколько это работоспособно XDDDDDDDD
            // тоесть если монстр может преследовать то он преследует, если он застрял в стене - то заслужил (принеяться нижний кусок)

            // Проверил, не работоспособно  нужны нормальные алгоритмы поиска краткого пути. Лень. Нижние 4 ифа бесполезны
            if (GameExtension.GetCreature(x, y + 1) is Gold || GameExtension.GetCreature(x, y + 1) is null || GameExtension.GetCreature(x, y + 1) is Player) //Вниз проверка
            {
                 return new CreatureCommand { DeltaX = 0, DeltaY = 1, TransformTo = null };
            }

            if (GameExtension.GetCreature(x - 1, y) is Gold || GameExtension.GetCreature(x - 1, y) is null || GameExtension.GetCreature(x - 1, y) is Player) //Влево проверка
            {
                 return new CreatureCommand { DeltaX = -1, DeltaY = 0, TransformTo = null };
            }

            if (GameExtension.GetCreature(x, y - 1) is Gold || GameExtension.GetCreature(x, y - 1) is null || GameExtension.GetCreature(x, y - 1) is Player) //Вверх проверка
            {
                  return new CreatureCommand { DeltaX = 0, DeltaY = -1, TransformTo = null };
            }

            if (GameExtension.GetCreature(x + 1, y) is Gold || GameExtension.GetCreature(x + 1, y) is null || GameExtension.GetCreature(x + 1, y) is Player) //Вправо проверка
            {
                  return new CreatureCommand { DeltaX = 1, DeltaY = 0, TransformTo = null };
            }



            return new CreatureCommand { DeltaX = 0, DeltaY = 0, TransformTo = null }; //в 4 четырёх стенах Бездействие
        }

        public int GetDrawingPriority()
        {
            return 2;
        }

        public bool DeadInConflict(ICreature conflictedObject)
        {
            if (conflictedObject.GetImageFileName() == "Digger.png")
            {
                return false;
            }
            else if (conflictedObject.GetImageFileName() == "Monster.png")
            {
                return true;
            }
            else if (conflictedObject.GetImageFileName() == "Sack.png")
            {
                return true;
            }
            return false;
        }
    }
    public class Mathtematic // Вычисление Расстояние между монстром и игроков
    {
        public static double Distation (int monsterX, int monsterY, int diggerX, int diggerY)
        {
            return Math.Sqrt(Math.Pow(diggerX - monsterX, 2) + Math.Pow(diggerY - monsterY, 2));
        }
    }
    public static class GameExtension
    {
        private static Boundary boundary = new Boundary(); 
        public static ICreature GetCreature(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Game.MapWidth || y >= Game.MapHeight)
                return boundary;
            return Game.Map[x, y];
        }
    }
    public class Boundary : ICreature
    {
        public string GetImageFileName()
        {
            return "Gold.png";
        }

        public CreatureCommand Act(int x, int y)
        {
            return new CreatureCommand { DeltaX = 0, DeltaY = 0, TransformTo = null };
        }

        public int GetDrawingPriority()
        {
            return 4;
        }

        public bool DeadInConflict(ICreature conflictedObject)
        {
            if (conflictedObject.GetImageFileName() == "Digger.png")
            {
                return true;
            }
            if (conflictedObject.GetImageFileName() == "Monster.png") return true;
            return false;
        }
    }
}

