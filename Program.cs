using Raylib_cs;

int fps = 12;
float spf = 1.0f / fps;

Raylib.InitWindow(500, 500, "snake-rl");

while (!Raylib.WindowShouldClose())
{
    input();
    update();
    render();
}

///////////

void input()
{

}

void update()
{

}

void render()
{
    Raylib.BeginDrawing();
    Raylib.EndDrawing();
}

///////////

enum Direction
{
    Up,
    Down,
    Left,
    Right,
}

enum BlockType
{
    Head,
    Body,
}

class Block
{

}

class Vec2d
{
    int X { get; set; } = 0;
    int Y { get; set; } = 0;
}

class Snake
{
    List<Block> Blocks { get; set; } = new();
    Direction Direction { get; set; }
    Block Head { get; set; } = new(BlockType.Head);

    public Snake()
    {
        Blocks.Add(Head);
    }

    public void ChangeDirection(Direction direction)
    {
        Direction = direction switch
        {
            Direction.Up =>  Direction == Direction.Down ? Direction : Direction.Up,
        };
    }
}

