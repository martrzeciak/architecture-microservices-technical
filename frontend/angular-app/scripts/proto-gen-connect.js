// Cross-platform proto:gen:connect script
// Generuje TypeScript stubs z .proto za pomocą protoc-gen-es + protoc-gen-connect-es
const { execSync } = require('child_process');
const path = require('path');
const os = require('os');

const isWindows = os.platform() === 'win32';

// Na Windows npm tworzy .cmd wrappery w node_modules/.bin
const bin = (name) =>
  path.join('node_modules', '.bin', isWindows ? `${name}.cmd` : name);

const protoDir = path.join('..', '..', 'protos');
const outDir = path.join('src', 'generated-connect');

const parts = [
  'grpc_tools_node_protoc',
  `--plugin=protoc-gen-es=${bin('protoc-gen-es')}`,
  `--es_out=${outDir}`,
  `--plugin=protoc-gen-connect-es=${bin('protoc-gen-connect-es')}`,
  `--connect-es_out=${outDir}`,
  `-I ${protoDir}`,
  path.join(protoDir, 'product.proto'),
  path.join(protoDir, 'order.proto'),
];

const cmd = parts.join(' ');
console.log('[proto:gen:connect]', cmd);
execSync(cmd, { stdio: 'inherit' });
