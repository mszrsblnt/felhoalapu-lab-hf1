FROM locustio/locust
WORKDIR /mnt/locust
COPY locustfile.py .
COPY test.jpg .
EXPOSE 8089
ENV LOCUST_WEB_PORT=8089

CMD ["-f", "/mnt/locust/locustfile.py"]