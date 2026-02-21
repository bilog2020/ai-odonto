import { useState, useEffect, useCallback } from 'react';
import {
  View,
  Text,
  FlatList,
  TouchableOpacity,
  StyleSheet,
  ActivityIndicator,
  Alert,
} from 'react-native';
import { useRouter } from 'expo-router';
import { api } from '../../services/api';

interface ChatSession {
  id: number;
  title: string;
  createdAt: string;
}

export default function ChatListScreen() {
  const router = useRouter();
  const [sessions, setSessions] = useState<ChatSession[]>([]);
  const [loading, setLoading] = useState(true);

  const loadSessions = useCallback(async () => {
    try {
      const data = await api.chat.getSessions();
      setSessions(data);
    } catch (e: any) {
      Alert.alert('Error', e.message ?? 'No se pudieron cargar las consultas.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadSessions();
  }, [loadSessions]);

  async function createSession() {
    try {
      const session = await api.chat.createSession('Nueva consulta');
      router.push(`/chat/${session.id}`);
    } catch (e: any) {
      Alert.alert('Error', e.message ?? 'No se pudo crear la consulta.');
    }
  }

  if (loading) {
    return (
      <View style={styles.center}>
        <ActivityIndicator size="large" color="#1a73e8" />
      </View>
    );
  }

  return (
    <View style={styles.container}>
      <FlatList
        data={sessions}
        keyExtractor={(item) => item.id.toString()}
        renderItem={({ item }) => (
          <TouchableOpacity
            style={styles.item}
            onPress={() => router.push(`/chat/${item.id}`)}
          >
            <Text style={styles.itemTitle}>{item.title}</Text>
            <Text style={styles.itemDate}>
              {new Date(item.createdAt).toLocaleDateString('es-AR')}
            </Text>
          </TouchableOpacity>
        )}
        ListEmptyComponent={
          <Text style={styles.empty}>No tenés consultas todavía.</Text>
        }
        contentContainerStyle={sessions.length === 0 ? styles.emptyContainer : undefined}
        onRefresh={loadSessions}
        refreshing={loading}
      />

      <TouchableOpacity style={styles.fab} onPress={createSession}>
        <Text style={styles.fabText}>+ Nueva consulta</Text>
      </TouchableOpacity>
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: '#f5f5f5' },
  center: { flex: 1, justifyContent: 'center', alignItems: 'center' },
  item: {
    backgroundColor: '#fff',
    marginHorizontal: 16,
    marginTop: 12,
    padding: 16,
    borderRadius: 8,
    borderWidth: 1,
    borderColor: '#eee',
  },
  itemTitle: { fontSize: 16, fontWeight: '500', color: '#1a1a2e' },
  itemDate: { fontSize: 12, color: '#999', marginTop: 4 },
  empty: { textAlign: 'center', color: '#999', fontSize: 16 },
  emptyContainer: { flex: 1, justifyContent: 'center' },
  fab: {
    backgroundColor: '#1a73e8',
    margin: 16,
    borderRadius: 8,
    padding: 16,
    alignItems: 'center',
  },
  fabText: { color: '#fff', fontSize: 16, fontWeight: '600' },
});
