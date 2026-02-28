
using Microsoft.EntityFrameworkCore;
using userPanelOMR.Context;
//using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using userPanelOMR.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using userPanelOMR.model;
using System.IdentityModel.Tokens.Jwt;
using System.Net.WebSockets;
using System.Security.Claims;
using userPanelOMR.WebSoket;
using Microsoft.Extensions.FileProviders;
using userPanelOMR.model.web;

namespace userPanelOMR
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Build Application
            var builder = WebApplication.CreateBuilder(args);

            // Add class depandancey to the container. 
            // GetConnectionString se hum connection string ko lete hain jo humne appsettings.json me define kiya hai then make ConnectionString
            // Add DbContext Service
            // builder.Services.AddDbContext<JWTContext>(options =>
            // options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
            // ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))));
            builder.Services.AddDbContext<JWTContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


            // Add Controller Service 
            builder.Services.AddControllers();
            builder.Services.AddScoped<HashEncption>();
            builder.Services.AddScoped<jwtTokenGen>();
            builder.Services.AddScoped<DynamicForm>();
            builder.Services.AddScoped<BlogImgSave>();
            builder.Services.AddTransient<userPanelOMR.Service.otpMail>();

            // Learn more about configuring Swagger/OpenAPI at
            builder.Services.AddEndpointsApiExplorer();  // Make meta data for get/post for swagger
            builder.Services.AddSwaggerGen();            // Gen UI Swagger

            var configration = builder.Configuration;
            builder.Services.AddJwtAuthentication(configration);

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAnyOrigin",
                policy =>
                {
                    policy
                    .WithOrigins("http://localhost:65236", "http://admin.helpinghandsanstha.in")
                    .AllowAnyOrigin()   // agr ko sepecific path hai front end ka to de do other wise all allow path. <un-secure>
                    .AllowAnyHeader()   // Front end se kisi bhi type k "Header" ko allow kar do.
                    .AllowAnyMethod();  // Front end se kisi bhi type k "Mathord" ko allow kar do.
                });
            });

            // Build Project
            var app = builder.Build();

            // Http Routing Redirection
            app.UseHttpsRedirection();

            // CROS ERROR Header
            // Jab Front end other port par chalta hai aur BackEnd other port par chalta hai to apne ko 
            // tab browser ko lagta hai dono alg alg server hai iske liye usko permission chahiye.
            // AddPolicy("allowAll") diya hai isliye isko cors k liye use krenge. 
            // program.cs me >< app.useCors("allowAll"); to sab controller par auto lag jayega.
            // program.cs me >< app.useCors(); to sab controller pr [EnableCors("allowAll")] dena hoga. <> ERROR

            app.UseCors("AllowAnyOrigin");


            // Use Authentication & Authorization Middlewares
            app.UseAuthentication();  
            app.UseAuthorization();
            app.MapControllers();


            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Syncfusion Key Add
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1NCaF5cXmZCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdnWXhdcHRVQmVeV0F3Wks=\r\n");


            // Permission Folder
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider( Path.Combine(Directory.GetCurrentDirectory(), "WebImg")),
                RequestPath = "/Uploads",
                OnPrepareResponse = ctx =>
                {
                    ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
                }
            });

            app.UseWebSockets();
            app.Use(async (context, next) =>
            {
                // make path, client ne header k sath req. bheji hai / ni.
                if (context.Request.Path == "/ws" && context.WebSockets.IsWebSocketRequest)
                {
                    // Check if the request is a WebSocket request
                    var socket = await context.WebSockets.AcceptWebSocketAsync();

                    // 2. Extract token from query string
                    var token = context.Request.Query["token"].ToString();
                    if (string.IsNullOrEmpty(token))
                    {
                        context.Response.StatusCode = 400; // Bad request
                        await context.Response.WriteAsync("Token missing");
                        return;
                    }

                    // 3. Parse JWT token to get userId
                    string userId = null;
                    try
                    {
                        var handler = new JwtSecurityTokenHandler();
                        var jwtToken = handler.ReadJwtToken(token);
                        userId = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "nameid")?.Value;
                    }
                    catch
                    {
                        context.Response.StatusCode = 401; // Unauthorized
                        await context.Response.WriteAsync("Invalid token");
                        return;
                    }

                    if (string.IsNullOrEmpty(userId))
                    {
                        context.Response.StatusCode = 401; // Unauthorized
                        await context.Response.WriteAsync("UserId not found in token");
                        return;
                    }

                    // 4. Get services
                    var connectionManager = context.RequestServices.GetRequiredService<wsConnetionManager>();
                    var handlerService = context.RequestServices.GetRequiredService<wsHandler>();

                    // 5. Add socket with userId
                    connectionManager.AddSocket(userId, socket);

                    var buffer = new byte[1024 * 4];

                    // Add the WebSocket to the connection manager
                    try
                    {
                        while (socket.State == WebSocketState.Open)
                        {
                            var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                            if (result.MessageType == WebSocketMessageType.Close)
                            {
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"WebSocket error: {ex.Message}");
                    }
                    finally
                    {
                        // 6. Remove socket on disconnect
                        connectionManager.RemoveSocket(userId);
                        await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                    }
                }
                else
                {
                    await next();
                }
            });

            try
            {
                app.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                throw;
            }
        }
    }
}
