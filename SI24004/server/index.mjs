import { createServer } from 'node:http';
import { handler } from '../.output/server/index.mjs';

const PORT = process.env.PORT || 3000;
createServer(handler).listen(PORT, () => {
  console.log(`Nuxt running on port ${PORT}`);
});
