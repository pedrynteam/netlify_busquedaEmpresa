
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using IdentityServer.Data;
using Microsoft.EntityFrameworkCore;
using HotChocolate.Types;
using HotChocolate;
using HotChocolate.Subscriptions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.IdentityModel.Logging;
using HotChocolate.Execution.Configuration;
using MGTModel.AuthService;
using MGTServices.AuthService;
using HotChocolate.AspNetCore;
using HotChocolate.AspNetCore.Playground;

namespace MGTServerAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
                         // Add DataAccessLayer
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")),
                    ServiceLifetime.Transient
                );

            // Add Identity Server            
            services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders()
                .AddTokenProvider("MyApp", typeof(DataProtectorTokenProvider<IdentityUser>));

            IdentityModelEventSource.ShowPII = true;    

            // Add in-memory event provider - This is for subscriptions. NuGet Package HotChocolate.Subscriptions.InMemory
            var eventRegistry = new InMemoryEventRegistry();
            services.AddSingleton<IEventRegistry>(eventRegistry);
            services.AddSingleton<IEventSender>(eventRegistry); //*/

            // Add GraphQL
            services
                .AddDataLoaderRegistry()
                .AddGraphQL((sp =>
                   SchemaBuilder.New()
                       .AddServices(sp)
                       .AddQueryType(d => d.Name("Query"))
                       .AddType<AuthServiceQueries>()
                       .AddMutationType(d => d.Name("Mutation"))
                       .AddType<AuthServiceMutations>()
                       /*.AddType<PersonMutations>()
                       .AddType<UserMutations>()
                       .AddSubscriptionType(d => d.Name("Subscription"))
                       .AddType<MessageSubscriptions>()
                       .AddType<PersonSubscriptions>()
                       .AddType<MessageExtension>()
                       .AddType<PersonExtension>()*/
                       .AddAuthorizeDirectiveType()
                       .BindClrType<string, StringType>()
                       .BindClrType<Guid, IdType>()
                       .Create()),
                        new QueryExecutionOptions
                        {
                            TracingPreference = TracingPreference.OnDemand
                        }
                    );

            // Add Authorization
            var tokenSection = Configuration.GetSection("JwtSettings");
            string APSecretKey = tokenSection["JwtSecurityKey"];
            string ValidateIssuer = "true";
            string ValidateAudience = "true";
            string ValidateLifetime = "true";
            string ValidateIssuerSigningKey = "true";
            string ValidIssuer = tokenSection["JwtIssuer"];
            string ValidAudience = tokenSection["JwtAudience"];

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = Convert.ToBoolean(ValidateIssuer),
                        ValidateAudience = Convert.ToBoolean(ValidateAudience),
                        ValidateLifetime = Convert.ToBoolean(ValidateLifetime),
                        ValidateIssuerSigningKey = Convert.ToBoolean(ValidateIssuerSigningKey),
                        ValidIssuer = ValidIssuer,
                        ValidAudience = ValidAudience,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(APSecretKey)),
                        TokenDecryptionKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(APSecretKey))
                    };
                });

            /*
            services.AddQueryRequestInterceptor((context, builder, ct) =>
            {

                /* To do something with the incoming user
                //var claims = ctx.User.Claims;

                System.Diagnostics.Debug.WriteLine("Claims ---->");
                foreach (var item in ctx.User.Claims)
                {
                    System.Diagnostics.Debug.WriteLine("Claim: " + item.Type + " - " + item.Value);
                }
                
                var identity = new ClaimsIdentity();
                identity.AddClaim(new Claim(ClaimTypes.Country, "us"));
                identity.AddClaim(new Claim("IsNewUser", "Yes"));                
                ctx.User.AddIdentity(identity);
                /

                return Task.CompletedTask;
            });*/

            services.AddQueryRequestInterceptor(async (context, builder, ct) =>
            {
                if (context.User.Identity.IsAuthenticated)
                {
                    await Task.Delay(1);
                    // The name in the token is the UserAppId
                    builder.AddProperty(
                        "currentUserId",
                        context.User.FindFirst(ClaimTypes.Name).Value);

                    builder.AddProperty(
                        "currentEmail",
                        context.User.FindFirst(ClaimTypes.Email).Value);

                    builder.AddProperty(
                        "currentIdentityId",
                        context.User.FindFirst("Identity").Value);

                    /*IPersonRepository personRepository =
                        context.RequestServices.GetRequiredService<IPersonRepository>();
                    await personRepository.UpdateLastSeenAsync(personId, DateTime.UtcNow, ct);*/
                }
            });

            services.AddAuthorization(options =>
            {
                // Using Token
                options.AddPolicy("IsNewUser", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim("IsUser", "Yes")
                ));
                //*/

                /*/ Reevalute DB claims, because GraphQl calls the method everytime, 
                // so if it's 5 users with one Auth Field, there will be 5 calls to the Auth Service and DB
                // Using ApplicationDbContext Database claims
                options.AddPolicy("IsNewUser", policy =>policy.Requirements.Add(
                    new ClaimInDatabaseRequirement(new Claim("UserType", "user")))
                );
                */

            });

            services.AddSingleton<IAuthorizationHandler, ClaimInDatabaseHandler>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            // enable Sockets if you want tu support GraphQL subscription
            //app.UseWebSockets();                        

            // enable GraphQL and playground.
            app.UseGraphQL("/graphql");
            app.UsePlayground(new PlaygroundOptions { QueryPath = "/graphql", Path = "/playground" }); // Navigate to https://localhost:44361/playground/

        }
    }
}
