namespace Gazeus.DesafioMatch3.Models
{
    public enum TileSpecialAction { None = 0, ClearLines = 1, Bomb = 2, ColorClear = 3, Death = 4}

    public class Tile
    {
        public int Id { get; set; }
        public int Type { get; set; }
        public TileSpecialAction Action { get; set; }
    }
}
