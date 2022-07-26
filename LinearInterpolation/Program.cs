using LinearInterpolation;


PInvoke.DisableScreenSaverAndLogOff();

var rand = new Random();
bool automationIsRunning;
var globalSleepTime = 10;
var resetWaiter = false;

// MouseHook for MouseMove. Update rate is 30 FPS
var myMouseHook = new MouseHook();
myMouseHook.MouseMove += MyMouseHookOnMouseMove;

while (true)
{
    globalSleepTime = 10;
    var screenSizeX = 3440;
    var screenSizeY = 1440;
    var offset = 200;

    var startPosition = PInvoke.GetCursorPosition();
    var targetPosition = new PointInt(rand.Next(offset, screenSizeX - offset), rand.Next(offset, screenSizeY - offset));
    Console.WriteLine($"O({startPosition.x}, {startPosition.y}) --> P({targetPosition.x}, {targetPosition.y})");

    LinearSmoothMove(startPosition, targetPosition, 1000);

    // Wait globalSleepTime seconds
    for (int i = globalSleepTime; i >= 0; i--)
    {
        if (resetWaiter)
        {
            i = globalSleepTime;
            resetWaiter = false;
        }
        Console.SetCursorPosition(0, Console.CursorTop);
        Console.Write("   ");
        Console.SetCursorPosition(0, Console.CursorTop);
        Console.Write($"{i:D2}");
        Thread.Sleep(TimeSpan.FromSeconds(1));
    }

    Console.WriteLine();
}


// Shows the linear interpolation with varied speed and time grid using a simulated mouse cursor movement. 
void LinearSmoothMove(PointInt startPoint, PointInt endPoint, int speed)
{
    var frameRate = 60;      // the speedfunction is resampled for a smooth movement and to avoid c# timing problems
    ushort lowerThreshold = 25; // full speed above 25% of the amount of samples
    ushort upperThreshold = 75; // full speed until 75% oft the amount of samples
    ushort minimumSpeed = 40;   // go along with at least 40% of the specified speed 

    automationIsRunning = true;
    var distance = (int) PointExtension.GetDistance(startPoint, endPoint); // distance is the amount of samples, btw.
    var stepSizeX = 1.0*(endPoint.x - startPoint.x) / distance;
    var stepSizeY = 1.0*(endPoint.y - startPoint.y) / distance;
    var itX = startPoint.x;
    var itY = startPoint.y;

    var speedFunction = LinearInterpolator.GetSampleArray((uint)distance, speed, lowerThreshold, upperThreshold, minimumSpeed);
    var resampled = LinearInterpolator.Resample(speedFunction, frameRate);

    Console.WriteLine($"Distance: {distance}, Time (est.): {speedFunction[^1].Item4:#0.000} seconds");

    var startTime = DateTime.UtcNow;
    uint samplesJumped = 0;
    foreach (var tuple in resampled)
    {
        if (!automationIsRunning)
        {
            var breakPos = PInvoke.GetCursorPosition();
            Console.WriteLine($"Break Position: {breakPos.x}, {breakPos.y}");
            return;
        }

        Thread.Sleep(TimeSpan.FromSeconds(1.0/ frameRate));
        itX += (int)(stepSizeX * (tuple.Item1 - samplesJumped));
        itY += (int)(stepSizeY * (tuple.Item1 - samplesJumped));
        samplesJumped = tuple.Item1;
        myMouseHook.SetMouseCursorManual(new PointInt(itX, itY));
    }

    var diff = DateTime.UtcNow - startTime;
    Console.WriteLine($"Time measured: {diff.TotalSeconds:#0.000}");
    automationIsRunning = false;
}


void MyMouseHookOnMouseMove(object sender, MouseHookEventArgs e)
{
    automationIsRunning = false;
    globalSleepTime = 60;
    resetWaiter = true;
}