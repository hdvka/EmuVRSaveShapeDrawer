using System.Drawing;
using Newtonsoft.Json;

namespace EmuVRShapeDrawer;

class Program
{
    public const double ps1DiscDistance = 0.118188 / 10;
    static void Main(string[] args)
    {
        var option = Operations.DrawDiscGrid;
        //var gridSize = 128;

        // Set the brightness threshold (0-255)

        if (args.Length != 1 || !int.TryParse(args[0], out int numBetween))
        {
            numBetween = 600;
        }

        string outPath = @"C:\EmuVR\Saved Data\Rooms\Slot11.json";
        string inPath = @"C:\EmuVR\Saved Data\Rooms\Slot11.json.startingPixel";
        Bitmap image = new Bitmap(@"C:\Users\skail\source\repos\EmuVRSaveShapeDrawer\EmuVRSaveShapeDrawer\retroRugToProcess.jpg");
        image.RotateFlip(RotateFlipType.Rotate90FlipY);

        var jsonStr = File.ReadAllText(inPath);

        var saveSlot = JsonConvert.DeserializeObject<SaveSlot>(jsonStr);
        var gamesToAdd = new List<Game>();


        if(option == Operations.Connect2Discs)
        {
            gamesToAdd = Connect2Discs(saveSlot, numBetween);
        }
        else if(option == Operations.DrawDiscGrid)
        {
            gamesToAdd = DrawDiscGrid(saveSlot, image);
        }

        saveSlot.games.Clear();
        saveSlot.games.AddRange(gamesToAdd);
                
        var jsonToWrite = JsonConvert.SerializeObject(saveSlot);

        using (StreamWriter sw = new StreamWriter(outPath))
        {
            sw.Write(jsonToWrite);
        }
    }

    private static List<Game> DrawDiscGrid(SaveSlot saveSlot, Bitmap image)
    {
        var startingCornerGame = saveSlot.games[0];
        int brightnessThreshold = 100;

        var pixelBasePath = "Games\\psx\\Pixels\\palette";
        var m3uExtension = ".m3u";

        var palette = new Palette();

        var gamesToAdd = new List<Game>();

        var pixelsWide = image.Width; // image.Width
        var pixelsHigh = image.Height; // image.Height

        for (int i = 0; i < pixelsWide; i++)
        {
            for (int j = 0; j < pixelsHigh; j++)
            {
                var pixel = image.GetPixel(i, j);

                var pixelPaletteIndex = palette.GetPaletteIndexEuclidean(pixel);
                var pixelTransparent = pixelPaletteIndex == -1;

                //var pixelAlpha = pixel.A;
                //var pixelTransparent = pixelAlpha < 10;

                //var pixelBrightness = image.GetPixel(i,j).GetBrightness() * 255;
                //var pixelTransparent = pixelBrightness > brightnessThreshold;

                if(!pixelTransparent)
                {
                        gamesToAdd.Add(
                            new Game{
                                position = new Position {
                                    x = startingCornerGame.position.x + (i * ps1DiscDistance),
                                    y = startingCornerGame.position.y + 0.0001,
                                    z = startingCornerGame.position.z + (j * ps1DiscDistance)
                                },
                                rotation = startingCornerGame.rotation,
                                scale = startingCornerGame.scale,
                                frozen = true,
                                path = pixelBasePath + pixelPaletteIndex + m3uExtension
                            }
                        );
                    //if(pixelBrightness > brightnessThreshold)
                    //{
                        //gamesToAdd.Add(
                            //new Game{
                                //position = new Position {
                                    //x = startingCornerGame.position.x + (i * ps1DiscDistance),
                                    //y = startingCornerGame.position.y + 0.1,
                                    //z = startingCornerGame.position.z + (j * ps1DiscDistance)
                                //},
                                //rotation = startingCornerGame.rotation,
                                //scale = startingCornerGame.scale,
                                //frozen = true,
                                //path = "Games\\psx\\Pixels\\palette4.m3u"
                            //}
                        //);
                    //}
                    //else
                    //{
                        //gamesToAdd.Add(
                            //new Game{
                                //position = new Position {
                                    //x = startingCornerGame.position.x + (i * ps1DiscDistance),
                                    //y = startingCornerGame.position.y + 0.1,
                                    //z = startingCornerGame.position.z + (j * ps1DiscDistance)
                                //},
                                //rotation = startingCornerGame.rotation,
                                //scale = startingCornerGame.scale,
                                //frozen = true,
                                //path = "Games\\psx\\Pixels\\palette7.m3u"
                            //}
                        //);
                    //}
                }
            }
        }

        return gamesToAdd;
    }

    private static List<Game> Connect2Discs(SaveSlot saveSlot, int numBetween)
    {
        var diskToClone = saveSlot.games[0];

        var gamesToAdd = new List<Game>();

        for (int i = 0; i < saveSlot.games.Count; i++)
        {
            if(i % 2 == 0)
            {
                var newGames = GetEquallySpacedGames(
                    saveSlot.games[i], saveSlot.games[i+1],
                    numBetween, diskToClone.path, true, false);

                foreach (var newGame in newGames)
                {
                    gamesToAdd.Add(newGame);
                }
            }
        }

        return gamesToAdd;
    }

    private static List<Game> GetEquallySpacedGames(
        Game game1, Game game2,
        int numPointsBetween, string path, 
        bool isLogarithmicScale, bool isExponentialScale)
    {
        List<Game> games = new List<Game>();

        // Calculate the distance between two game objects
        double distanceX = game2.position.x - game1.position.x;
        double distanceY = game2.position.y - game1.position.y;
        double distanceZ = game2.position.z - game1.position.z;

        // Calculate the interval between two points
        double intervalX = distanceX / (numPointsBetween + 1);
        double intervalY = distanceY / (numPointsBetween + 1);
        double intervalZ = distanceZ / (numPointsBetween + 1);

        // Calculate the interval for rotation and scale attributes
        double rotIntervalX = (game2.rotation.x - game1.rotation.x) / (numPointsBetween + 1);
        double rotIntervalY = (game2.rotation.y - game1.rotation.y) / (numPointsBetween + 1);
        double rotIntervalZ = (game2.rotation.z - game1.rotation.z) / (numPointsBetween + 1);
        double rotIntervalW = (game2.rotation.w - game1.rotation.w) / (numPointsBetween + 1);

        // Calculate the interval for scale attributes
        double scaleXInterval, scaleYInterval, scaleZInterval;
        if (isLogarithmicScale)
        {
            scaleXInterval = Math.Pow(game2.scale.x / game1.scale.x, 1.0 / (numPointsBetween + 1));
            scaleYInterval = Math.Pow(game2.scale.y / game1.scale.y, 1.0 / (numPointsBetween + 1));
            scaleZInterval = Math.Pow(game2.scale.z / game1.scale.z, 1.0 / (numPointsBetween + 1));
        }
        else
        {
            scaleXInterval = (game2.scale.x - game1.scale.x) / (numPointsBetween + 1);
            scaleYInterval = (game2.scale.y - game1.scale.y) / (numPointsBetween + 1);
            scaleZInterval = (game2.scale.z - game1.scale.z) / (numPointsBetween + 1);
        }

        //double scaleXInterval = (game2.scale.x - game1.scale.x) / (numPointsBetween + 1);
        //double scaleYInterval = (game2.scale.y - game1.scale.y) / (numPointsBetween + 1);
        //double scaleZInterval = (game2.scale.z - game1.scale.z) / (numPointsBetween + 1);

        // Add equally spaced games to the list
        for (int i = 1; i <= numPointsBetween; i++)
        {
            double posX = game1.position.x + (i * intervalX);
            double posY = game1.position.y + (i * intervalY);
            double posZ = game1.position.z + (i * intervalZ);

            double rotX = game1.rotation.x + (i * rotIntervalX);
            double rotY = game1.rotation.y + (i * rotIntervalY);
            double rotZ = game1.rotation.z + (i * rotIntervalZ);
            double rotW = game1.rotation.w + (i * rotIntervalW);

            
            double scaleX = game1.scale.x;
            double scaleY = game1.scale.y;
            double scaleZ = game1.scale.z;


            if (isLogarithmicScale)
            {
                scaleX = game1.scale.x * Math.Pow(scaleXInterval, i);
                scaleY = game1.scale.y * Math.Pow(scaleYInterval, i);
                scaleZ = game1.scale.z * Math.Pow(scaleZInterval, i);
            }
            else if (isExponentialScale)
            {
                scaleX *= Math.Pow(2, i * scaleXInterval);
                scaleY *= Math.Pow(2, i * scaleYInterval);
                scaleZ *= Math.Pow(2, i * scaleZInterval);
            }
            else
            {
                scaleX = game1.scale.x + (i * scaleXInterval);
                scaleY = game1.scale.y + (i * scaleYInterval);
                scaleZ = game1.scale.z + (i * scaleZInterval);
            }

            //double scaleX = game1.scale.x + (i * scaleXInterval);
            //double scaleY = game1.scale.y + (i * scaleYInterval);
            //double scaleZ = game1.scale.z + (i * scaleZInterval);

            games.Add(new Game
            {
                position = new Position
                {
                    x = posX,
                    y = posY,
                    z = posZ
                },
                rotation = new Rotation
                {
                    x = rotX,
                    y = rotY,
                    z = rotZ,
                    w = rotW
                },
                scale = new Scale
                {
                    x = scaleX,
                    y = scaleY,
                    z = scaleZ
                },
                frozen = true,
                path = path
            });
        }

        return games;
    }

}




public enum Operations
{
    Connect2Discs,
    DrawDiscGrid
}
