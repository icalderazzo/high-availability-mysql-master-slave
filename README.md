# MySql Master-Slave

Este repositorio contiene un ejemplo de un sistema de órdenes de alta disponibilidad basado en una base de datos MySQL en modalidad **master-slave** con **ProxySQL** como capa de enrutamiento inteligente de queries. El ejemplo consta de los siguientes componentes:

* **mysql master** — nodo primario, recibe todas las escrituras, carga transaccional y lecturas simples.
* **mysql slave** — réplica para lecturas complejas, sincronizada por replicación nativa de MySQL.
* **proxySQL** — proxy SQL que rutea las queries entre master y slave de forma transparente.
* **orderAPI** — servicio .NET de ejemplo que se conecta a la base de datos a través de ProxySQL.

**Pre requisitos**
* Docker

## Arquitectura general

El objetivo del sistema es separar la carga de trabajo: el **master** atiende las operaciones transaccionales (escrituras y lecturas en tiempo real) y el **slave** absorbe las queries pesadas de reporting (stored procedures), descargando así al master. ProxySQL hace este ruteo de forma transparente, sin que la aplicación necesite saber a qué nodo se conecta.

```
┌─────────────────────────────────────────────────────────────┐
│                      Capa de Aplicación                       │
│   OrderAPI (.NET) o cualquier cliente MySQL                   │
│   Conexión única → proxysql:6033                              │
└───────────────────────────────┬───────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────┐
│                         ProxySQL                              │
│   Query Router                                                │
│   • Regla 10:  ^CALL  → Slave  (Hostgroup 1)                  │
│   • Regla 100: .*     → Master (Hostgroup 0)                  │
│   Puertos: 6033 (MySQL) · 6032 (Admin)                        │
└──────────────────┬───────────────────────┬────────────────────┘
                   │                       │
         ┌─────────▼─────────┐   ┌─────────▼─────────┐
         │   MySQL Master    │   │   MySQL Slave     │
         │   Port: 3306      │   │   Port: 3307      │
         │   Hostgroup: 0    │   │   Hostgroup: 1    │
         │                   │   │                   │
         │ • SELECT          │   │ • CALL            │
         │ • INSERT          │   │   (stored procs)  │
         │ • UPDATE          │──▶│   Replicación     │
         │ • DELETE          │   │                   │
         │ • DDL             │   │                   │
         └───────────────────┘   └───────────────────┘
```

### Estrategia de ruteo

ProxySQL evalúa el patrón de cada query y la dirige al hostgroup correspondiente:

| Tipo de query      | Patrón                  | Destino                | Ejemplo                                   |
|--------------------|-------------------------|------------------------|-------------------------------------------|
| Stored Procedures  | `^CALL`                 | Slave (Hostgroup 1)    | `CALL GetMonthlySalesReport(2003, 1)`     |
| SELECT             | `^SELECT`               | Master (Hostgroup 0)   | `SELECT * FROM orders`                    |
| INSERT             | `^INSERT`               | Master (Hostgroup 0)   | `INSERT INTO orders VALUES (...)`         |
| UPDATE             | `^UPDATE`               | Master (Hostgroup 0)   | `UPDATE orders SET status = 'Shipped'`    |
| DELETE             | `^DELETE`               | Master (Hostgroup 0)   | `DELETE FROM orders WHERE id = 1`         |
| DDL                | `^CREATE\|ALTER\|DROP`  | Master (Hostgroup 0)   | `CREATE TABLE ...`                        |

La idea es que las consultas de reporting (encapsuladas en stored procedures e invocadas con `CALL`) se ejecuten en el slave, mientras que el master se enfoca en la carga transaccional.

## Sample database

El nodo master está precargado con un backup de [classicmodels](data-samples/mysqltutorial.org/mysql-classicmodesl.sql) sample database: [mysqltutorial.org](http://www.mysqltutorial.org/mysql-sample-database.aspx). El archivo SQL se monta como `entrypoint` en `/docker-entrypoint-initdb.d/` y MySQL lo ejecuta automáticamente **en el primer startup**, cuando el volumen está vacío.

> **Note:** Si el volumen `mysql_master_data` ya existe de un init previo, **no** se va a re ejecutar. Es necesario eliminar los volúmenes para que el seed funcione correctamente:
> ```
> docker compose down -v
> ```

## Arquitectura del setup automático

Todo el setup de replicación se realiza automáticamente al levantar el entorno, sin pasos manuales:

```
mysql-master
  ├── Seed: classicmodels.sql  (docker-entrypoint-initdb.d/01-seed)
  └── Init: init.sql           (docker-entrypoint-initdb.d/02-replication)
        └── GRANT REPLICATION SLAVE TO replication_user & CREATE OrdersApp user

mysql-slave  (depends_on: mysql-master healthy)
  └── init.sh
        ├── Starts MySQL in background
        ├── Waits for master to be healthy
        ├── mysqldump --all-databases --master-data=1  →  seeds all data
        │     (embeds CHANGE MASTER TO with exact binlog position)
        ├── CHANGE MASTER TO + START SLAVE
        └── MySQL runs in foreground (replication active)

proxysql  (depends_on: mysql-master & mysql-slave)
  └── proxysql.cnf
        ├── Define hostgroups: 0 (master) y 1 (slave)
        ├── Carga las reglas de ruteo (CALL → slave, resto → master)
        └── Expone el puerto 6033 a la aplicación
```

## Levantar el entorno

```bash
docker compose up --build -d
```

Eso es todo. El entorno se configura automáticamente:
1. El master arranca, ejecuta el seed de `classicmodels` y otorga privilegios de replicación al usuario `replication_user` (además de crear el usuario `ordersapp` para la aplicación).
2. El slave espera a que el master esté healthy (healthcheck cada 5s, hasta 20 reintentos).
3. El slave ejecuta `mysqldump --master-data=1` desde el master para obtener todos los datos y la posición exacta del binlog.
4. El slave importa el dump, configura `CHANGE MASTER TO` y ejecuta `START SLAVE`.
5. ProxySQL arranca, registra master y slave en sus hostgroups y carga las reglas de ruteo. La aplicación se conecta únicamente a `proxysql:6033`.

Verificá que los tres servicios estén arriba:

```bash
docker compose ps
```

## Verificar el estado de la replicación

### Desde el master
```bash
docker exec -it mysql-master mysql -uroot -pyour_super_secure_root_password -e "SHOW MASTER STATUS\G"
```

### Desde el slave
```bash
docker exec -it mysql-slave mysql -uroot -pyour_super_secure_root_password -e "SHOW SLAVE STATUS\G"
```

Los campos clave a verificar:
- `Slave_IO_Running: Yes`
- `Slave_SQL_Running: Yes`
- `Last_Error:` (debe estar vacío)

## Credenciales

| Parámetro         | Valor                          |
|-------------------|-------------------------------|
| Root password     | `your_super_secure_root_password` |
| Replication user  | `replication_user`            |
| Replication pass  | `replication_password_123`    |
| OrdersApp user    | `ordersapp`                   |
| OrdersApp pass    | `ordersapp_password`          |
| Master port       | `3306`                        |
| Slave port        | `3307`                        |
| ProxySQL port     | `6033`                        |

## Teardown

```bash
# Detener y eliminar contenedores + volúmenes (permite re-seed limpio)
docker compose down -v
```
