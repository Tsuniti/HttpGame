using HttpGame.Entities;
using System.Net;
using System.Text;

HttpListenerContext? firstPlayerContext = null;
HttpListenerContext? secondPlayerContext = null;
Player firstPlayer = new Player();
Player secondPlayer = new Player();
int scoreToWin = 3;
char? firstMove = null;
char? secondMove = null;


async Task WaitForPlayersAsync()
{
    await Task.Run(async () =>
    {
        HttpListener connectorListener = new HttpListener();
        connectorListener.Prefixes.Add("http://localhost:5000/connect/");
        connectorListener.Start();

        await Console.Out.WriteLineAsync("Waiting for players");
        firstPlayerContext = await connectorListener.GetContextAsync();
        await Console.Out.WriteLineAsync("First player connected!");

        secondPlayerContext = await connectorListener.GetContextAsync();
        await Console.Out.WriteLineAsync("Second player connected!");
    });
}
await WaitForPlayersAsync();
byte[] firstPlayerRes = Encoding.UTF8.GetBytes(
    $"Your id is {firstPlayer.Id}. Use it to move\n" +
    $"\nHint: curl http://localhost:5000/game/ -H \"PlayerId: {firstPlayer.Id}\" -d \"R/P/S\"");
byte[] secondPlayerRes = Encoding.UTF8.GetBytes(
    $"Your id is {secondPlayer.Id}. Use it to move\n" +
    $"\nHint: curl http://localhost:5000/game/ -H \"PlayerId: {secondPlayer.Id}\" -d \"R/P/S\"");


byte[] res = Encoding.UTF8.GetBytes(
        "First player's move" +
        "\nHint: curl -d \"R\" -X POST http://localhost:5000/game/");



await firstPlayerContext.Response.OutputStream.WriteAsync(firstPlayerRes, 0, firstPlayerRes.Length);
firstPlayerContext.Response.Close();

await secondPlayerContext.Response.OutputStream.WriteAsync(secondPlayerRes, 0, secondPlayerRes.Length);
secondPlayerContext.Response.Close();

Console.WriteLine("test");
async Task WaitForMovesAsync()
{
    HttpListener moveListener = new HttpListener();
    moveListener.Prefixes.Add("http://localhost:5000/game/");
    moveListener.Start();
    while (firstPlayer.Score < scoreToWin && secondPlayer.Score < scoreToWin)
    {
        await Task.Run(async () =>
        {
            
            firstMove = null;
            secondMove = null;


            while (firstMove == null)
            {
                firstPlayerContext = await moveListener.GetContextAsync();

                //if (firstPlayerContext.Request.Headers["PlayerId"] != firstPlayer.Id.ToString() &&
                //    firstPlayerContext.Request.Headers["PlayerId"] != secondPlayer.Id.ToString())
                //{
                //    byte[] res = Encoding.UTF8.GetBytes("Wrong PlayerId");
                //    await firstPlayerContext.Response.OutputStream.WriteAsync(res, 0, res.Length);
                //    firstPlayerContext.Response.Close();
                //    continue;
                //}
                using (var reader = new StreamReader(firstPlayerContext.Request.InputStream, firstPlayerContext.Request.ContentEncoding))
                {
                    string requestBody = await reader.ReadToEndAsync();
                    if (requestBody[0] == 'R' || requestBody[0] == 'P' || requestBody[0] == 'S')
                    {
                        firstMove = requestBody[0];
                    }
                    else
                    {
                        byte[] res = Encoding.UTF8.GetBytes("Wrong move");
                        await firstPlayerContext.Response.OutputStream.WriteAsync(res, 0, res.Length);
                        firstPlayerContext.Response.Close();
                        continue;
                    }
                }
            };
            await Console.Out.WriteLineAsync("First player moved");

            while (secondMove == null)
            {
                secondPlayerContext = await moveListener.GetContextAsync();

                //if (secondPlayerContext.Request.Headers["PlayerId"] != firstPlayer.Id.ToString() &&
                //    secondPlayerContext.Request.Headers["PlayerId"] != secondPlayer.Id.ToString())
                //{
                //    byte[] res = Encoding.UTF8.GetBytes("Wrong PlayerId");
                //    await secondPlayerContext.Response.OutputStream.WriteAsync(res, 0, res.Length);
                //    secondPlayerContext.Response.Close();
                //    continue;
                //}
                //if (secondPlayerContext.Request.Headers["PlayerId"] == firstPlayerContext.Request.Headers["PlayerId"])
                //{
                //    byte[] res = Encoding.UTF8.GetBytes("This player has already moved");
                //    await secondPlayerContext.Response.OutputStream.WriteAsync(res, 0, res.Length);
                //    secondPlayerContext.Response.Close();
                //    continue;
                //}

                using (var reader = new StreamReader(secondPlayerContext.Request.InputStream, secondPlayerContext.Request.ContentEncoding))
                {
                    string requestBody = await reader.ReadToEndAsync();
                    if (requestBody[0] == 'R' || requestBody[0] == 'P' || requestBody[0] == 'S')
                    {
                        secondMove = requestBody[0];
                    }
                    else
                    {
                        byte[] res = Encoding.UTF8.GetBytes("Wrong move");
                        await secondPlayerContext.Response.OutputStream.WriteAsync(res, 0, res.Length);
                        secondPlayerContext.Response.Close();
                        continue;
                    }
                }
            };

        });

        if (firstPlayer.Id.ToString() != firstPlayerContext.Request.Headers["PlayerId"])
        {
            char? temp = firstMove;
            firstMove = secondMove;
            secondMove = temp;
        }

        if (firstMove == secondMove)
        {
            byte[] res = Encoding.UTF8.GetBytes("It's a tie");
            await firstPlayerContext.Response.OutputStream.WriteAsync(res, 0, res.Length);
            firstPlayerContext.Response.Close();
            await secondPlayerContext.Response.OutputStream.WriteAsync(res, 0, res.Length);
            secondPlayerContext.Response.Close();
            continue;
        }
        if (firstMove == 'R' && secondMove == 'S' ||
            firstMove == 'S' && secondMove == 'P' ||
            firstMove == 'P' && secondMove == 'R')
        {
            firstPlayer.Score++;

            byte[] res = Encoding.UTF8.GetBytes(
                $"PlayerId: {firstPlayer.Id} wins!\n" +
                $"Score:\n" +
                $"Player{firstPlayer.Id} score: {firstPlayer.Score}\n" +
                $"Player{secondPlayer.Id} score: {secondPlayer.Score}\n");


            await firstPlayerContext.Response.OutputStream.WriteAsync(res, 0, res.Length);
            firstPlayerContext.Response.Close();
            await secondPlayerContext.Response.OutputStream.WriteAsync(res, 0, res.Length);
            secondPlayerContext.Response.Close();
        }
        else
        {
            secondPlayer.Score++;

            byte[] res = Encoding.UTF8.GetBytes(
                $"PlayerId: {secondPlayer.Id} wins!\n" +
                $"Score:\n" +
                $"Player{firstPlayer.Id} score: {firstPlayer.Score}\n" +
                $"Player{secondPlayer.Id} score: {secondPlayer.Score}\n");


            await firstPlayerContext.Response.OutputStream.WriteAsync(res, 0, res.Length);
            firstPlayerContext.Response.Close();
            await secondPlayerContext.Response.OutputStream.WriteAsync(res, 0, res.Length);
            secondPlayerContext.Response.Close();
        }

    }

}
await WaitForMovesAsync();
