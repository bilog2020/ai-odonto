import { useState, useEffect, useRef, useCallback } from 'react';
import {
  View,
  Text,
  FlatList,
  TextInput,
  TouchableOpacity,
  StyleSheet,
  ActivityIndicator,
  KeyboardAvoidingView,
  Platform,
  Alert,
} from 'react-native';
import { useLocalSearchParams } from 'expo-router';
import { api } from '../../services/api';

interface Message {
  role: string;
  content: string;
  sourceChunkIds: number[];
}

export default function ChatScreen() {
  const { id } = useLocalSearchParams<{ id: string }>();
  const sessionId = parseInt(id, 10);
  const [messages, setMessages] = useState<Message[]>([]);
  const [input, setInput] = useState('');
  const [sending, setSending] = useState(false);
  const [loading, setLoading] = useState(true);
  const listRef = useRef<FlatList>(null);

  const loadSession = useCallback(async () => {
    try {
      const data = await api.chat.getSession(sessionId);
      setMessages(data.messages ?? []);
    } catch (e: any) {
      Alert.alert('Error', e.message ?? 'No se pudo cargar la sesión.');
    } finally {
      setLoading(false);
    }
  }, [sessionId]);

  useEffect(() => {
    loadSession();
  }, [loadSession]);

  async function sendMessage() {
    const text = input.trim();
    if (!text || sending) return;

    const userMsg: Message = { role: 'user', content: text, sourceChunkIds: [] };
    setMessages((prev) => [...prev, userMsg]);
    setInput('');
    setSending(true);

    try {
      const reply = await api.chat.sendMessage(sessionId, text);
      const assistantMsg: Message = {
        role: 'assistant',
        content: reply.content,
        sourceChunkIds: reply.sourceChunkIds ?? [],
      };
      setMessages((prev) => [...prev, assistantMsg]);
    } catch (e: any) {
      Alert.alert('Error', e.message ?? 'No se pudo enviar el mensaje.');
      setMessages((prev) => prev.slice(0, -1));
    } finally {
      setSending(false);
      setTimeout(() => listRef.current?.scrollToEnd({ animated: true }), 100);
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
    <KeyboardAvoidingView
      style={styles.container}
      behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
      keyboardVerticalOffset={90}
    >
      <FlatList
        ref={listRef}
        data={messages}
        keyExtractor={(_, i) => i.toString()}
        renderItem={({ item }) => (
          <View
            style={[
              styles.bubble,
              item.role === 'user' ? styles.userBubble : styles.assistantBubble,
            ]}
          >
            <Text style={item.role === 'user' ? styles.userText : styles.assistantText}>
              {item.content}
            </Text>
            {item.role === 'assistant' && item.sourceChunkIds.length > 0 && (
              <Text style={styles.sources}>
                Fuentes: {item.sourceChunkIds.join(', ')}
              </Text>
            )}
          </View>
        )}
        ListEmptyComponent={
          <Text style={styles.emptyChat}>Hacé tu primera consulta odontológica.</Text>
        }
        contentContainerStyle={styles.messageList}
        onContentSizeChange={() => listRef.current?.scrollToEnd({ animated: false })}
      />

      <View style={styles.inputRow}>
        <TextInput
          style={styles.input}
          placeholder="Escribí tu consulta..."
          value={input}
          onChangeText={setInput}
          multiline
          maxLength={1000}
          returnKeyType="send"
          onSubmitEditing={sendMessage}
        />
        <TouchableOpacity
          style={[styles.sendButton, (!input.trim() || sending) && styles.sendDisabled]}
          onPress={sendMessage}
          disabled={!input.trim() || sending}
        >
          {sending ? (
            <ActivityIndicator color="#fff" size="small" />
          ) : (
            <Text style={styles.sendText}>Enviar</Text>
          )}
        </TouchableOpacity>
      </View>
    </KeyboardAvoidingView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: '#f5f5f5' },
  center: { flex: 1, justifyContent: 'center', alignItems: 'center' },
  messageList: { padding: 16, flexGrow: 1 },
  bubble: {
    maxWidth: '80%',
    borderRadius: 12,
    padding: 12,
    marginBottom: 8,
  },
  userBubble: {
    backgroundColor: '#1a73e8',
    alignSelf: 'flex-end',
  },
  assistantBubble: {
    backgroundColor: '#fff',
    alignSelf: 'flex-start',
    borderWidth: 1,
    borderColor: '#eee',
  },
  userText: { color: '#fff', fontSize: 15 },
  assistantText: { color: '#1a1a2e', fontSize: 15 },
  sources: {
    fontSize: 11,
    color: '#999',
    marginTop: 4,
    fontStyle: 'italic',
  },
  emptyChat: {
    textAlign: 'center',
    color: '#999',
    fontSize: 15,
    marginTop: 60,
  },
  inputRow: {
    flexDirection: 'row',
    padding: 12,
    backgroundColor: '#fff',
    borderTopWidth: 1,
    borderColor: '#eee',
    alignItems: 'flex-end',
  },
  input: {
    flex: 1,
    backgroundColor: '#f5f5f5',
    borderRadius: 20,
    paddingHorizontal: 16,
    paddingVertical: 10,
    fontSize: 15,
    maxHeight: 100,
    marginRight: 8,
  },
  sendButton: {
    backgroundColor: '#1a73e8',
    borderRadius: 20,
    paddingHorizontal: 16,
    paddingVertical: 10,
    justifyContent: 'center',
  },
  sendDisabled: { backgroundColor: '#aaa' },
  sendText: { color: '#fff', fontWeight: '600' },
});
