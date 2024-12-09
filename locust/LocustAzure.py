from locust import task, run_single_user
from locust import FastHttpUser


class Orders(FastHttpUser):
    # host = "https://brewupapi.ambitiousocean-9c685401.italynorth.azurecontainerapps.io"
    host = "https://localhost:44328"
    default_headers = {
        "Accept": "*/*",
        "Accept-Encoding": "gzip, deflate, br",
        "Accept-Language": "en-US,en;q=0.5",
        "Connection": "keep-alive",
        "Host": "localhost:5294",
        "Sec-Fetch-Dest": "empty",
        "Sec-Fetch-Mode": "cors",
        "Sec-Fetch-Site": "same-origin",
        "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:123.0) Gecko/20100101 Firefox/123.0",
    }

    @task
    def t(self):
        with self.rest("GET", "/v1/brewup/orders") as resp:
            pass

class Beers(FastHttpUser):
    # host = "https://brewupapi.ambitiousocean-9c685401.italynorth.azurecontainerapps.io"
    host = "https://localhost:44328"

    @task
    def t(self):
        with self.rest("GET", "/v1/brewup/beers") as resp:
            pass

if __name__ == "__main__":
    run_single_user(Orders)
    run_single_user(Beers)
