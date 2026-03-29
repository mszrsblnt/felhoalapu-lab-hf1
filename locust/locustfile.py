from locust import HttpUser, task, between
import random, string

class PhotoGalleryUser(HttpUser):
    wait_time = between(1, 3)
    gallery_id = None

    def on_start(self):
        # 1. Regisztráció és bejelentkezés
        email = f"user_{''.join(random.choices(string.ascii_lowercase, k=6))}@test.com"
        password = "Password123!"
        
        self.client.post("/register", json={"email": email, "password": password}, name="/register")
        res = self.client.post("/login", json={"email": email, "password": password}, name="/login")
        
        if res.status_code == 200:
            token = res.json().get("accessToken")
            self.client.headers.update({"Authorization": f"Bearer {token}"})
            
            # 2. Egyedi galéria létrehozása minden usernek (hogy ne ütközzenek)
            gal_name = f"Galéria_{''.join(random.choices(string.ascii_letters, k=8))}"
            self.client.post("/api/Galleries", json={"name": gal_name, "isPublic": True}, name="/api/Galleries")
            
            galleries = self.client.get("/api/Galleries?mine=true", name="/api/Galleries").json()
            if galleries:
                # Az utoljára létrehozottat vesszük el
                self.gallery_id = galleries[0].get("id")

    @task(5)
    def upload_and_view(self):
        """Feltöltés és azonnali megtekintés - a legsűrűbb funkció szimulálása"""
        if not self.gallery_id: return
        
        # Feltöltés
        with open("test.jpg", "rb") as image:
            files = {"File": ("test.jpg", image.read(), "image/jpeg")}
        data = {"Name": f"Kép_{random.randint(1,100000)}"}
        
        post_res = self.client.post(f"/api/photos?galleryId={self.gallery_id}", data=data, files=files, name="POST /api/photos")
        
        # Ha sikerült, megpróbáljuk a listázást és az utolsó kép megtekintését
        if post_res.status_code == 200:
            res = self.client.get(f"/api/photos?galleryId={self.gallery_id}&sortBy=date", name="GET /api/photos")
            if res.status_code == 200 and res.json():
                photo_id = res.json()[0].get("id")
                self.client.get(f"/api/photos/{photo_id}/view?galleryId={self.gallery_id}", name="GET /api/photos/{id}/view")

    @task(1)
    def photo_cleanup(self):
        """Törlés manuális hibakezeléssel (catch_response), hogy tiszta maradjon a statisztika"""
        if not self.gallery_id: return
        
        res = self.client.get(f"/api/photos?galleryId={self.gallery_id}", name="GET /api/photos")
        if res.status_code == 200 and res.json():
            photo_id = res.json()[0].get("id")
            
            with self.client.delete(f"/api/photos/{photo_id}?galleryId={self.gallery_id}", 
                                    name="DELETE /api/photos/{id}", 
                                    catch_response=True) as response:
                # Ha 200 (siker) vagy 404 (valaki már törölte), akkor sikeresnek könyveljük el
                if response.status_code in [200, 204, 404]:
                    response.success()
                else:
                    response.failure(f"Váratlan hiba: {response.status_code}")

    @task(1)
    def gallery_lifecycle(self):
        """Galéria törlése és új létrehozása (teljes életciklus tesztelése)"""
        if not self.gallery_id: return
        
        # Töröljük a jelenlegit
        with self.client.delete(f"/api/Galleries/{self.gallery_id}", name="/api/Galleries/{id}", catch_response=True) as response:
            if response.status_code in [200, 404]:
                response.success()
                self.gallery_id = None
        
        # Hozunk létre egy újat a helyére
        gal_name = f"New_Gal_{random.randint(1,99999)}"
        res = self.client.post("/api/Galleries", json={"name": gal_name, "isPublic": True}, name="/api/Galleries")
        if res.status_code == 200:
            galleries = self.client.get("/api/Galleries?mine=true", name="/api/Galleries").json()
            if galleries:
                self.gallery_id = galleries[0].get("id")