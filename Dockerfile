# define a imagem base
FROM debian:10

# define o mantenedor da imagem
LABEL maintainer="Rondinele Guimarães"

#Atualiza a imagem com os pacotes e faz instalação"
RUN apt-get update && apt-get upgrade -y && apt-get install gpg -y\
    && apt-get install nginx -y && apt-get install wget -y


# Adicionar chave e feed do repositório da Microsoft
RUN wget -O - https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor > microsoft.asc.gpg \
    && mv microsoft.asc.gpg /etc/apt/trusted.gpg.d/ \
    && wget https://packages.microsoft.com/config/debian/10/prod.list \
    && mv prod.list /etc/apt/sources.list.d/microsoft-prod.list \
    chown root:root /etc/apt/trusted.gpg.d/microsoft.asc.gpg \
    && chown root:root /etc/apt/sources.list.d/microsoft-prod.list

#Instalar o SDK do .NET Core
RUN  apt-get update && sudo apt-get install apt-transport-https \
    && apt-get update && apt-get install dotnet-sdk-3.1

#Instalar o ASP.NET Core Runtime
RUN  apt-get update && apt-get install apt-transport-https \
    && apt-get update &&  apt-get install aspnetcore-runtime-3.1

#Instalar o tempo de execução do .NET Core
RUN  apt-get update &&  apt-get install apt-transport-https \
    && apt-get update && apt-get install dotnet-runtime-3.1

# Expoe a porta 80
EXPOSE 80
# Comando para iniciar o NGINX no Container
CMD ["nginx", "-g", "daemon off;"]
