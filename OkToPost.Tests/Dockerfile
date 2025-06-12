FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["OkToPost.Tests/OkToPost.Tests.csproj", "OkToPost.Tests/"]
COPY ["OkToPost/OkToPost.csproj", "OkToPost/"]
RUN dotnet restore "OkToPost.Tests/OkToPost.Tests.csproj"
COPY . .

WORKDIR /src/OkToPost.Tests

# ✅ Run tests directly during the build stage
CMD ["dotnet", "test", "--logger:console;verbosity=normal"]
