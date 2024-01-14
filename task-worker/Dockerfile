FROM mcr.microsoft.com/dotnet/sdk:8.0

RUN curl ifconfig.me

WORKDIR ~
RUN mkdir app
COPY ./src/ ./app/src/
COPY ./task-orchestrator.csproj ./app/task-orchestrator.csproj
COPY ./Program.cs ./app/Program.cs

ENTRYPOINT ["tail", "-f", "/dev/null"]