const dbName = process.env.MONGO_DB;
const appUser = process.env.MONGO_APP_USER;
const appPass = process.env.MONGO_APP_PASS;

if (!dbName || !appUser || !appPass) {
  print("Missing env vars for Mongo init (MONGO_DB/MONGO_APP_USER/MONGO_APP_PASS)");
  quit(1);
}

db = db.getSiblingDB(dbName);

db.createUser({
  user: appUser,
  pwd: appPass,
  roles: [
    { role: "readWrite", db: dbName }
  ]
});

print(`Created app user '${appUser}' for db '${dbName}'`);
