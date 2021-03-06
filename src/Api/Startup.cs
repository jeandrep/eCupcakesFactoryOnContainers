﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SignalRDemo.Hubs;
using Confluent.Kafka;
using MediatR;
namespace Api
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            var builder = new ConfigurationBuilder()
            .AddConfiguration(configuration)
            .AddJsonFile("globalkafkasettings.json")
            .AddEnvironmentVariables();
            _configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddCors(c =>
                {
                    c.AddPolicy("AllowOrigin", options => options.WithOrigins("http://localhost:3000","http://35.232.19.14","https://35.232.19.14").AllowAnyMethod().AllowAnyHeader().AllowCredentials());
                });
            
            var producerConfig = new ProducerConfig();
            _configuration.Bind("producer",producerConfig);


            //TODO::Reading the environment variable.
            var envBootStrapServers = _configuration.GetValue<string>("ENV_KAFKA_CLUSTER");
            if(!String.IsNullOrEmpty(envBootStrapServers)){
                producerConfig.BootstrapServers =  envBootStrapServers;
            }

            var envSaslUserName = _configuration.GetValue<string>("ENV_KAFKA_USER_NAME");
            if(!String.IsNullOrEmpty(envSaslUserName)){
                producerConfig.SaslUsername =  envSaslUserName;
            }

            var envSaslPassword = _configuration.GetValue<string>("ENV_KAFKA_USER_PASSWORD");
            if(!String.IsNullOrEmpty(envSaslPassword)){
                producerConfig.SaslPassword =  envSaslPassword;
            }

            services.AddSingleton<ProducerConfig>(producerConfig);
            services.AddMediatR(typeof(Startup));
 
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }
            app.UseCors("AllowOrigin");
            app.UseMvc();
        }
    }
}
