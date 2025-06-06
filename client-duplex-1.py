import random
import agentpy as ap
import socket
import time

class AgenteSocket(ap.Agent):

    def setup(self):
        self.pos = (
            self.model.random.uniform(0, 5),
            self.model.random.uniform(0, 5),
            self.model.random.uniform(0, 5)
        )

    def enviar_posicion_a_unity(self):
        try:
            s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            s.connect((self.p.host, self.p.puerto))

            bienvenida = s.recv(1024).decode("ascii")
            print(f"[Agente {self.id}] Mensaje del servidor Unity: {bienvenida}")

            mensaje = f"POS:{self.pos[0]},{self.pos[1]},{self.pos[2]}<EOF>"
            s.send(mensaje.encode("ascii"))

            s.close()
            print(f"[Agente {self.id}] Posición enviada a Unity: {self.pos}")

        except Exception as e:
            print(f"[Agente {self.id}] Error al conectar con Unity: {e}")

    def step(self):
        self.enviar_posicion_a_unity()
        time.sleep(1)


class ModeloDeRed(ap.Model):

    def setup(self):
        self.agentes = ap.AgentList(self, self.p.n_agentes, AgenteSocket)

    def step(self):
        self.agentes.step()

    def update(self):
        pass

    def end(self):
        print("Simulación finalizada.")

params = {
    'n_agentes': 1,
    'host': '127.0.0.1',
    'puerto': 1201,
}

if __name__ == "__main__":
    model = ModeloDeRed(params)
    results = model.run()
