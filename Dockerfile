FROM gitpod/workspace-full:latest

USER root

RUN wget -q https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb && \
    dpkg -i packages-microsoft-prod.deb && rm -rf packages-microsoft-prod.deb && \
    add-apt-repository universe && \
    apt-get update && apt-get -y -o APT::Install-Suggests="true" install dotnet-sdk-2.2 && \
    apt -y clean;
    
RUN wget -O - https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor > microsoft.asc.gpg &&\
 mv microsoft.asc.gpg /etc/apt/trusted.gpg.d/ &&\
 wget https://packages.microsoft.com/config/debian/10/prod.list &&\
 mv prod.list /etc/apt/sources.list.d/microsoft-prod.list &&\
 chown root:root /etc/apt/trusted.gpg.d/microsoft.asc.gpg &&\
 chown root:root /etc/apt/sources.list.d/microsoft-prod.list
 
RUN  apt-get update && sudo apt-get install apt-transport-https &&\
 apt-get update &&  apt-get install dotnet-sdk-3.1 -y
 
RUN  apt-get update && apt-get install apt-transport-https &&\
     apt-get update && apt-get install aspnetcore-runtime-3.1 -y
     
RUN apt-get update && apt-get install apt-transport-https &&\
  apt-get update &&  apt-get install dotnet-runtime-3.1 -y
