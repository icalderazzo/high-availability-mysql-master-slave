# MySql Master-Slave

Este repositorio contiene un ejemplo de configuración de MySQL en modalidad master-slave con 2 contenedores de Docker, con **setup completamente automatizado**.

**Pre requisitos**
* Docker
* Docker Desktop

## Sample database

El nodo master está precargado con un backup de [classicmodels](data-samples/mysqltutorial.org/mysql-classicmodesl.sql) sample database: [mysqltutorial.org](http://www.mysqltutorial.org/mysql-sample-database.aspx). El archivo SQL se monta como `entrypoint` en `/docker-entrypoint-initdb.d/` y MySQL lo ejecuta automáticamente **en el primer startup**, cuando el volumen está vacío.

> **Note:** Si el volumen `mysql_master_data` ya existe de un init previo, **no** se va a re ejecutar. Es necesario eliminar los volúmenes para que el seed funcione correctamente:
> ```
> docker compose down -v
> ```

## Arquitectura del setup automático

```
mysql-master
  ├── Seed: classicmodels.sql  (docker-entrypoint-initdb.d/01-seed)
  └── Init: init.sql           (docker-entrypoint-initdb.d/02-replication)
        └── GRANT REPLICATION SLAVE TO replication_user

mysql-slave  (depends_on: mysql-master healthy)
  └── init.sh
        ├── Starts MySQL in background
        ├── Waits for master to be healthy
        ├── mysqldump --all-databases --master-data=1  →  seeds all data
        │     (embeds CHANGE MASTER TO with exact binlog position)
        ├── CHANGE MASTER TO + START SLAVE
        └── MySQL runs in foreground (replication active)
```

## Levantar el entorno

```bash
docker compose up -d
```

Eso es todo. El slave se configura automáticamente:
1. El master arranca, ejecuta el seed de `classicmodels` y otorga privilegios de replicación al usuario `replication_user`.
2. El slave espera a que el master esté healthy (healthcheck cada 5s, hasta 20 reintentos).
3. El slave ejecuta `mysqldump --master-data=1` desde el master para obtener todos los datos y la posición exacta del binlog.
4. El slave importa el dump, configura `CHANGE MASTER TO` y ejecuta `START SLAVE`.

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
| Master port       | `3306`                        |
| Slave port        | `3307`                        |

## Teardown

```bash
# Detener y eliminar contenedores + volúmenes (permite re-seed limpio)
docker compose down -v
```
