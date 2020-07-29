FROM nginx

COPY ./nginx.conf /etc/nginx/nginx.conf

# Add permissions for nginx user.
# Thanks to Sophia Brandt at https://dev.to/sophiabrandt/run-docker-nginx-as-non-root-user-2b6b for these permissions.
RUN chown -R 9999:9999 /var/cache/nginx && \
    chown -R 9999:9999 /var/log/nginx && \
    chown -R 9999:9999 /etc/nginx/conf.d

RUN touch /var/run/nginx.pid && \
    chown -R 9999:9999 /var/run/nginx.pid

USER 9999
