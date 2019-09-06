FROM microsoft/dotnet:2.1-sdk AS base

MAINTAINER Riccardo Terrell

WORKDIR /app

COPY ./bin/RELEASE/netcoreapp2.1/publish/ /app

EXPOSE 8090

# Needed because https://stackoverflow.com/questions/51977474/install-dotnet-core-tool-dockerfile
# ENV PATH="${PATH}:/root/.dotnet/tools"

# RUN pbm help

CMD ["dotnet", "AkkaFractal.Remote.dll"]




